using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public interface IMissionControllerProvider
    {
        IMissionController Provide(MissionObjectiveType type, MissionDefinition missionData);
    }
    
    public class MissionControllerProvider : MonoBehaviour, IMissionControllerProvider
    {
        public IMissionController Provide(MissionObjectiveType type, MissionDefinition missionData)
        {
            switch (type)
            {
                case MissionObjectiveType.FinishLevel:
                    return new CompleteLevelMissionController(missionData);
                default:
                    return null;
            }
        }
    }
}