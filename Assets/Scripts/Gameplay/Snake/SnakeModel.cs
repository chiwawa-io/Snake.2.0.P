using System.Collections.Generic;
using UnityEngine;

public class SnakeModel
{
    public List<Vector2Int> Body = new();
    public Vector2Int Direction = Vector2Int.up;
    public Vector2Int PendingDirection = Vector2Int.up;
    
    public bool IsInvulnerable; 
    public bool IsRespawning;   
    public float MoveFrequency; 
    
    public int GemsCollected;
    public int SpeedUpsCollected;
}