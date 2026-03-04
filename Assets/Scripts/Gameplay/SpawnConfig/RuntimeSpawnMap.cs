using System.Collections.Generic;
using Core.Enums;
using Services.RNG;
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

        public void ShuffleAll(IRngService rng)
        {
            foreach (var list in _map.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    int randomIndex = rng.NextInt(i, list.Count);
                    Vector2Int temp = list[i];
                    list[i] = list[randomIndex];
                    list[randomIndex] = temp;
                }
            }
        }

        public void ShuffleAll()
        {
            foreach (var list in _map.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    int randomIndex = Random.Range(i, list.Count);
                    Vector2Int temp = list[i];
                    list[i] = list[randomIndex];
                    list[randomIndex] = temp;
                }
            }
        }
    }
}