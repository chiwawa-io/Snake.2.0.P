using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

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
    private readonly Dictionary<Vector2Int, ActiveItem> _activeItems = new();
    private readonly Dictionary<string, int> _lastSpawnPointIndex = new();
    
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
    [SerializeField] private SpawnPattern _portalsPattern;
    [SerializeField] private List<SpawnPattern> _easyChallengePatterns = new();
    [SerializeField] private List<SpawnPattern> _mediumChallengePatterns = new();
    [SerializeField] private List<SpawnPattern> _hardChallengePatterns = new();
    [SerializeField, Range(0, 1)] private float _challengeChance = 0.2f;

    private List<Vector2Int> _playerBodyPositions;
    private Vector2Int _gridBounds;
    
    private bool _isSpawningActive;
    
    private SpawnPointMap _currentSpawnPointMap;
    private SpawnPattern _currentChallengePattern;
    
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
        _isSpawningActive = true;
        
        _signalBus.Subscribe<ItemDestroyedSignal>(OnItemDestroyedByTimer);

        ChoosePatternsForDifficulty(difficulty);
        
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

        if (Random.value < _challengeChance && _currentChallengePattern != null)
        {
            SpawnPattern(_currentChallengePattern);
        }
        else
        {
            SpawnPattern(_regularFoodPattern);
        }
    }
    
    public ActiveItem GetItemAt(Vector2Int position) => _activeItems.GetValueOrDefault(position);

    public void RemoveItem(Vector2Int position)
    {
        _activeItems.Remove(position);
    }

    private void SpawnItem(string itemName, Vector2Int spawnPos)
    {
        GameItem itemData = _itemTypes.FirstOrDefault(i => i.objName == itemName);
        if (itemData == null)
        {
            Debug.LogError($"Item '{itemName}' not found in Item Types list.");
            return;
        }

        var instance = _container.InstantiatePrefab(itemData.prefab, (Vector2)spawnPos, Quaternion.identity, null);
        _activeItems[spawnPos] = new ActiveItem(itemData, instance);
    }
    
    private void SpawnPattern(SpawnPattern pattern)
    {
        if (pattern == null || _currentSpawnPointMap == null || pattern.items.Count == 0) return;

        string primaryItemName = pattern.items[0].itemName;
        List<Vector2Int> validPoints = _currentSpawnPointMap.GetPointsFor(primaryItemName);
        if (validPoints == null || validPoints.Count == 0) return;
        if (!_lastSpawnPointIndex.ContainsKey(primaryItemName)) _lastSpawnPointIndex[primaryItemName] = -1;

        for (int i = 0; i < validPoints.Count; i++)
        {
            int currentIndex = (_lastSpawnPointIndex[primaryItemName] + 1 + i) % validPoints.Count;
            Vector2Int anchorPos = validPoints[currentIndex];

            if (IsPatternLocationValid(pattern, anchorPos))
            {
                foreach (var item in pattern.items)
                {
                    SpawnItem(item.itemName, anchorPos + item.relativePosition);
                }
                _lastSpawnPointIndex[primaryItemName] = currentIndex;
                return;
            }
        }
    }
    
    private bool IsPatternLocationValid(SpawnPattern pattern, Vector2Int anchorPos)
    {
        foreach (var item in pattern.items)
        {
            Vector2Int pos = anchorPos + item.relativePosition;
            if (Mathf.Abs(pos.x) >= _gridBounds.x || Mathf.Abs(pos.y) >= _gridBounds.y) return false;
            if (_playerBodyPositions.Contains(pos) || _activeItems.ContainsKey(pos)) return false;
        }
        return true;
    }

    private void ChoosePatternsForDifficulty(string difficulty)
    {
        T PickRandom<T>(List<T> pool) => (pool?.Count > 0) ? pool[Random.Range(0, pool.Count)] : default;

        _currentSpawnPointMap = PickRandom(_spawnPointMaps);

        _currentChallengePattern = difficulty switch
        {
            "Easy" => PickRandom(_easyChallengePatterns),
            "Medium" => PickRandom(_mediumChallengePatterns),
            "Hard" => PickRandom(_hardChallengePatterns),
            _ => PickRandom(_easyChallengePatterns)
        };
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