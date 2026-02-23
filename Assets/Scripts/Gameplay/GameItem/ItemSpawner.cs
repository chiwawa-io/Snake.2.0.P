using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Events;
using Gameplay.SpawnConfig;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Gameplay.GameItem
{
    public class ActiveItem
    {
        public GameItem Data { get; }
        public GameObject Instance { get; }

        public ActiveItem(GameItem data, GameObject instance)
        {
            Data = data;
            Instance = instance;
        }
    }

    public class ItemSpawner : MonoBehaviour
    {
        private readonly Dictionary<ItemType, int> _lastSpawnPointIndex = new();
        private readonly Dictionary<Vector2Int, ActiveItem> _activeItems = new();

        [Header("Item Database")] 
        [SerializeField] private List<GameItem> _itemTypes;

        [Header("Spawning Intervals")] 
        [SerializeField] private int _speedUpSpawnDelay = 7;
        [SerializeField] private int _speedupSpawnInterval = 14;
        [SerializeField] private int _invulnerabilitySpawnDelay = 5;
        [SerializeField] private int _invulnerabilitySpawnInterval = 25;

        [Header("Pattern & Spawning Logic")] 
        [SerializeField] private List<SpawnPointMap> _spawnPointMaps = new();

        [SerializeField] private SpawnPattern _startingFoodPattern;
        [SerializeField] private SpawnPattern _speedUpPattern;
        [SerializeField] private SpawnPattern _invulnerabilityPattern;
        [SerializeField] private SpawnPattern _regularFoodPattern;
        [SerializeField] private List<SpawnPattern> _easyChallengePatterns = new();
        [SerializeField] private List<SpawnPattern> _mediumChallengePatterns = new();
        [SerializeField] private List<SpawnPattern> _hardChallengePatterns = new();
        [SerializeField, Range(0, 1)] private float _challengeChance = 0.4f;

        private List<Vector2Int> _playerBodyPositions;
        private Vector2Int _gridBounds;
        private string _currentDifficulty; 
        private bool _isSpawningActive;

        private SpawnPointMap _currentSpawnPointMap;

        private SignalBus _signalBus;
        private DiContainer _container;

        [Inject]
        public void Construct(SignalBus signalBus, DiContainer container)
        {
            _signalBus = signalBus;
            _container = container;
        }

        public void Initialize(Vector2Int gridBounds, List<Vector2Int> playerBody, string difficulty)
        {
            _gridBounds = gridBounds;
            _playerBodyPositions = playerBody;
            _currentDifficulty = difficulty;
            _isSpawningActive = true;

            _signalBus.Subscribe<ItemDestroyedSignal>(OnItemDestroyedByTimer);

            // Only pick the Map once per level
            _currentSpawnPointMap = _spawnPointMaps != null && _spawnPointMaps.Count > 0 
                ? _spawnPointMaps[Random.Range(0, _spawnPointMaps.Count)] 
                : null;

            if (_startingFoodPattern != null)
            {
                SpawnPattern(_startingFoodPattern);
            }

            StartCoroutine(SpawnSpeedUpCoroutine());
            StartCoroutine(SpawnInvulnerabilityCoroutine());
        }

        public void ResetSpawner()
        {
            StopAllCoroutines();
            _isSpawningActive = false;
            _signalBus.TryUnsubscribe<ItemDestroyedSignal>(OnItemDestroyedByTimer);

            foreach (ActiveItem item in _activeItems.Values.ToList())
            {
                if (item.Instance != null) Destroy(item.Instance);
            }

            _activeItems.Clear();
            _lastSpawnPointIndex.Clear();
        }

        public void OnFoodCollected()
        {
            if (!_isSpawningActive) return;

            bool spawnedSuccessfully = false;

            // 1. Try to spawn a challenge pattern
            if (Random.value < _challengeChance)
            {
                SpawnPattern challenge = GetRandomChallenge(_currentDifficulty);
                if (challenge != null)
                {
                    spawnedSuccessfully = SpawnPattern(challenge);
                }
            }

            // 2. FALLBACK: If challenge failed (or RNG said no), spawn regular food
            if (!spawnedSuccessfully)
            {
                spawnedSuccessfully = SpawnPattern(_regularFoodPattern);
                
                // 3. EXTREME EDGE CASE: Board is completely full
                if (!spawnedSuccessfully)
                {
                    Debug.LogWarning("Spawner failed to find any valid location! Board might be full.");
                }
            }
        }

        public ActiveItem GetItemAt(Vector2Int position) => _activeItems.GetValueOrDefault(position);

        public void RemoveItem(Vector2Int position)
        {
            _activeItems.Remove(position);
        }

        public bool HasActiveObstacles()
        {
            foreach (var activeItem in _activeItems.Values)
            {
                if (activeItem.Data.isObstacle) return true;
            }
            return false;
        }
        
        private void SpawnItem(ItemType type, Vector2Int spawnPos)
        {
            GameItem itemData = _itemTypes.FirstOrDefault(i => i.type == type);
            
            if (itemData == null)
            {
                Debug.LogError($"Item '{type}' not found in Item Types list.");
                return;
            }

            var instance = _container.InstantiatePrefab(itemData.prefab, (Vector2)spawnPos, Quaternion.identity, null);
            _activeItems[spawnPos] = new ActiveItem(itemData, instance);
        }

        private bool SpawnPattern(SpawnPattern pattern)
        {
            if (pattern == null || _currentSpawnPointMap == null || pattern.items.Count == 0) return false;

            ItemType primaryType = GetSmartAnchorType(pattern);
            
            List<Vector2Int> validPoints = _currentSpawnPointMap.GetPointsFor(primaryType);
            if (validPoints == null || validPoints.Count == 0)
            {
                Debug.LogWarning("Failed to get valid points for:" + primaryType.ToString() + _currentDifficulty + pattern.name + _currentSpawnPointMap);
                validPoints = _currentSpawnPointMap.GetPointsFor(ItemType.Food);
            }

            if (validPoints == null || validPoints.Count == 0) return false;

            if (!_lastSpawnPointIndex.ContainsKey(primaryType)) 
                _lastSpawnPointIndex[primaryType] = -1;

            for (int i = 0; i < validPoints.Count; i++)
            {
                int currentIndex = (_lastSpawnPointIndex[primaryType] + 1 + i) % validPoints.Count;
                Vector2Int anchorPos = validPoints[currentIndex];

                if (IsPatternLocationValid(pattern, anchorPos))
                {
                    foreach (var item in pattern.items)
                    {
                        SpawnItem(item.type, anchorPos + item.relativePosition);
                    }
                    _lastSpawnPointIndex[primaryType] = currentIndex;
                    return true; // Success!
                }
            }
            
            Debug.Log("Failed to spawn, reason: Not Enough Space");
            return false; 
        }

        private bool IsPatternLocationValid(SpawnPattern pattern, Vector2Int anchorPos)
        {
            int xBound = _gridBounds.x / 2;
            int yBound = _gridBounds.y / 2;

            foreach (var item in pattern.items)
            {
                Vector2Int pos = anchorPos + item.relativePosition;
                
                if (Mathf.Abs(pos.x) > xBound || Mathf.Abs(pos.y) > yBound) return false;
                if (_playerBodyPositions.Contains(pos) || _activeItems.ContainsKey(pos)) return false;
            }

            return true;
        }

        private ItemType GetSmartAnchorType(SpawnPattern pattern)
        {
            if (pattern.items.Any(i => i.type == ItemType.PreciousFood)) 
                return ItemType.PreciousFood;

            return pattern.items[0].type;
        }

        private SpawnPattern GetRandomChallenge(string difficulty)
        {
            List<SpawnPattern> pool = difficulty switch
            {
                "Easy" => _easyChallengePatterns,
                "Medium" => _mediumChallengePatterns,
                "Hard" => _hardChallengePatterns,
                _ => _easyChallengePatterns
            };

            if (pool != null && pool.Count > 0)
            {
                return pool[Random.Range(0, pool.Count)];
            }
            return null;
        }

        private void OnItemDestroyedByTimer(ItemDestroyedSignal signal)
        {
            _activeItems.Remove(signal.ItemPosition);
        }

        private IEnumerator SpawnSpeedUpCoroutine()
        {
            yield return new WaitForSeconds(_speedUpSpawnDelay);
            while (_isSpawningActive)
            {
                SpawnPattern(_speedUpPattern);
                yield return new WaitForSeconds(_speedupSpawnInterval);
            }
        }

        private IEnumerator SpawnInvulnerabilityCoroutine()
        {
            yield return new WaitForSeconds(_invulnerabilitySpawnDelay);
            while (_isSpawningActive)
            {
                SpawnPattern(_invulnerabilityPattern);
                yield return new WaitForSeconds(_invulnerabilitySpawnInterval);
            }
        }
    }
}