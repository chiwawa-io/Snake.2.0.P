using Luxodd.Game.HelpersAndUtils.Utils;
using Luxodd.Game.Scripts.Missions;

namespace Luxodd.Game.Scripts.HelpersAndUtils.Missions
{
    public class FinishLevelEvent : IEventData { }
    public class MissionProgressEvent : IEventData
    {
        public MissionObjectiveType Type { get; set; }
        public int Value { get; set; }
    }
}
