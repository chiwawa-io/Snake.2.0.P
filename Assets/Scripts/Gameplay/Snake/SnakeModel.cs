using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Snake
{
    public class SnakeModel
    {
        public List<Vector2Int> Body = new();
        public Vector2Int Direction = Vector2Int.up;
        public Vector2Int PendingDirection = Vector2Int.up;
        public Vector2Int LastTailPosition { get; set; }

        public bool IsInvulnerable;
        public bool IsRespawning;
        public float MoveFrequency;

        public int GemsCollected;
        public int SpeedUpsCollected;
    }
}