using Core.Enums;
using UnityEngine;

namespace Gameplay.GameItems
{
    [CreateAssetMenu(fileName = "New GameItem", menuName = "Snake/GameItem")]
    public class GameItem : ScriptableObject
    {
        [Header("Identity")]
        public ItemType type; // Replaced string objName
        public GameObject prefab;
        
        [Header("Collection Effect")]
        public bool isCollectible;
        public int scoreValue;

        [Header("Obstacle Effect")]
        public bool isObstacle;
        
        [Header("Power-Up Effect")]
        public bool isPowerUp;
        public PowerUpEffectType effectType; 
        public float effectDuration;
    }

    public enum PowerUpEffectType
    {
        None,
        SpeedUp,
        SlowDown,
        Invulnerable,
    }
}