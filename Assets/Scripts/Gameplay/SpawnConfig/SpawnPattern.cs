using Core.Enums;
using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.SpawnConfig
{
    [System.Serializable]
    public struct PatternItem
    {
        public ItemType type; 
        public Vector2Int relativePosition;
    }

    [CreateAssetMenu(fileName = "New Spawn Pattern", menuName = "Snake/Spawn Pattern")]
    public class SpawnPattern : ScriptableObject
    {
        public List<PatternItem> items = new List<PatternItem>();
    }
}