using Core.Enums;
using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.SpawnConfig
{
    [System.Serializable]
    public class SpawnPointList
    {
        public ItemType type; 
        public List<Vector2Int> positions;
    }

    [CreateAssetMenu(fileName = "New Spawn Point Map", menuName = "Snake/Spawn Point Map")]
    public class SpawnPointMap : ScriptableObject
    {
        public List<SpawnPointList> spawnPoints;

        public List<Vector2Int> GetPointsFor(ItemType type)
        {
            foreach (var list in spawnPoints)
            {
                if (list.type == type)
                {
                    return list.positions;
                }
            }
            return null;
        }
    }
}