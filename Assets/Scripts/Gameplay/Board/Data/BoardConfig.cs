using Core.Enums;
using UnityEngine;

namespace Gameplay.Global.Data
{
    [CreateAssetMenu(fileName = "New Board Config", menuName = "Snake/Board Config" )]
    public class BoardConfig : ScriptableObject
    {
        public GameDifficulty gameDifficulty;
        public Vector2Int boardSize;
    }
}