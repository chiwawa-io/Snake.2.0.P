using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Global.Data
{
    [CreateAssetMenu(fileName = "New Level Board Config", menuName = "Snake/Level board config")]
    public class LevelBoardsConfig : ScriptableObject
    {
        public List<BoardConfig> boardConfigs;
    }
}