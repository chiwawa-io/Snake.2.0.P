using Luxodd.Game.Scripts.Missions;

namespace UI.MissionCompletion.Data
{
    public struct MissionCompletionUIData
    {
        public string MissionName;
        public string Description;
        public MissionType Type;
        public bool IsCompleted;
        public float Payout; 
    }
}