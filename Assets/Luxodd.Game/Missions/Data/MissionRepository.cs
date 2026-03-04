using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    [CreateAssetMenu(fileName = "MissionRepository", menuName = "Create/Mission/Mission Repository")]
    public class MissionRepository : ScriptableObject
    {
        [field: SerializeField] public List<MissionDefinition> MissionDataList { get; private set; } = new List<MissionDefinition>();
    }
}