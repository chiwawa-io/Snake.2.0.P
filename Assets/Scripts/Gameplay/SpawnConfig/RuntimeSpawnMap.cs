using System.Collections.Generic;
using Core.Enums;
using UnityEngine;

namespace Gameplay.SpawnConfig
{
    public class RuntimeSpawnMap
    {
        private readonly Dictionary<ItemType, List<Vector2Int>> _map = new();

        public void AddPoint(ItemType type, Vector2Int position)
        {
            if (!_map.ContainsKey(type))
            {
                _map[type] = new List<Vector2Int>();
            }
            _map[type].Add(position);
        }

        public List<Vector2Int> GetPointsFor(ItemType type)
        {
            return _map.GetValueOrDefault(type, new List<Vector2Int>());
        }
    }
}