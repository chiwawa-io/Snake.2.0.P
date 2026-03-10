using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Events;
using Gameplay.SpawnConfig;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Gameplay.GameItems
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
        [Header("Item Database")] 
        [SerializeField] private List<GameItem> _itemTypes;
        
        [Header("Spawning Intervals")] 
        [SerializeField] private int _speedUpSpawnDelay = 7;
        [SerializeField] private int _speedupSpawnInterval = 14;
        [SerializeField] private int _invulnerabilitySpawnDelay = 5;
        [SerializeField] private int _invulnerabilitySpawnInterval = 25;
        
        [Header("Pattern Config")] 
        [SerializeField] private SpawnPattern _startingFoodPattern;
        [SerializeField] private SpawnPattern _speedUpPattern;
        [SerializeField] private SpawnPattern _slowDownPattern;
        [SerializeField] private SpawnPattern _invulnerabilityPattern;
        [SerializeField] private SpawnPattern _regularFoodPattern;
        [SerializeField] private List<SpawnPattern> _easyChallengePatterns = new();
        [SerializeField] private List<SpawnPattern> _mediumChallengePatterns = new();
        [SerializeField] private List<SpawnPattern> _hardChallengePatterns = new();
        [SerializeField, Range(0, 1)] private float _challengeChance = 0.4f;

        private readonly Dictionary<ItemType, int> _lastSpawnPointIndex = new();
        private readonly Dictionary<Vector2Int, ActiveItem> _activeItems = new();
        
        private List<Vector2Int> _playerBodyPositions;
        private Vector2Int _gridBounds;
        private GameDifficulty _currentDifficulty;
        private bool _isSpawningActive;

        private RuntimeSpawnMap _currentSpawnPointMap;
        private SpawnMapGenerator _mapGenerator;
        private SignalBus _signalBus;
        private DiContainer _container;

        [Inject]
        public void Construct(SignalBus signalBus, DiContainer container, SpawnMapGenerator mapGenerator)
        {
            _signalBus = signalBus;
            _container = container;
            _mapGenerator = mapGenerator;
        }

        public void Initialize(Vector2Int gridBounds, List<Vector2Int> playerBody, GameDifficulty difficulty, bool isSbSession)
        {
            _gridBounds = gridBounds;
            _playerBodyPositions = playerBody;
            _currentDifficulty = difficulty;
            _isSpawningActive = true;

            _signalBus.Subscribe<ItemDestroyedSignal>(OnItemDestroyedByTimer);
  
            _currentSpawnPointMap = _mapGenerator.Generate(_gridBounds, isSbSession);

            if (_startingFoodPattern != null)
            {
                SpawnPattern(_startingFoodPattern);
            }

            if (isSbSession)
            {
                _invulnerabilitySpawnDelay = 1500;
                _challengeChance = 0;
            }
            
            var currentSpeedModifierPattern = isSbSession ? _slowDownPattern : _speedUpPattern;
            StartCoroutine(SpawnSpeedModifierCoroutine(currentSpeedModifierPattern));
            
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

        public void SetSpawnChallengeChance(float probability)
        {
            _challengeChance = probability;
        }

        public void OnFoodCollected()
        {
            if (!_isSpawningActive) return;

            bool spawnedSuccessfully = false;

            if (Random.value < _challengeChance)
            {
                SpawnPattern challenge = GetRandomChallenge(_currentDifficulty);
                if (challenge != null)
                {
                    spawnedSuccessfully = SpawnPattern(challenge);
                }
            }

            if (!spawnedSuccessfully)
            {
                spawnedSuccessfully = SpawnPattern(_regularFoodPattern);
                
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
                validPoints = _currentSpawnPointMap.GetPointsFor(ItemType.Food);
            }

            if (validPoints == null || validPoints.Count == 0) 
            {
                return false;
            }

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
                    return true; 
                }
            }
            
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

            if (pattern.items.Any(i => i.type == ItemType.Food)) 
                return ItemType.Food;

            return pattern.items[0].type;
        }

        private SpawnPattern GetRandomChallenge(GameDifficulty difficulty)
        {
            List<SpawnPattern> pool = difficulty switch
            {
                GameDifficulty.Easy => _easyChallengePatterns,
                GameDifficulty.Medium => _mediumChallengePatterns,
                GameDifficulty.Hard => _hardChallengePatterns,
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

        private IEnumerator SpawnSpeedModifierCoroutine(SpawnPattern patternToSpawn)
        {
            yield return new WaitForSeconds(_speedUpSpawnDelay);
            while (_isSpawningActive)
            {
                SpawnPattern(patternToSpawn); // Use the parameter!
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