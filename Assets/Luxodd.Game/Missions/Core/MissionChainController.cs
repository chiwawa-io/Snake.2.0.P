using System.Collections.Generic;
using Luxodd.Game.HelpersAndUtils.Utils;

namespace Luxodd.Game.Scripts.Missions
{
    public interface IMissionChainInfo
    {
        IReadOnlyProperty<MissionState> State { get; }
        IReadOnlyProperty<MissionRewardState> RewardState { get; }
        
        MissionChainData MissionData { get; }

        int ChainId { get; }
        MissionObjectiveType Type { get; }
    }

    public interface IMissionChainController : IMissionChainInfo
    {
        void SetComplete();
        void SetInProgress();
        void TakeReward();
        void SetFailed();
        void SetPause();

    }

    public class MissionChainController: IMissionChainController
    {
        public IReadOnlyProperty<MissionState> State => _state;
        public IReadOnlyProperty<MissionRewardState> RewardState => _rewardState;
        public MissionChainData MissionData { get; }
        public int ChainId { get; }
        public MissionObjectiveType Type { get; }

        private readonly CustomProperty<MissionRewardState> _rewardState =
            new CustomProperty<MissionRewardState>(MissionRewardState.NotTaken);
        
        private readonly CustomProperty<MissionState> _state = new CustomProperty<MissionState>(MissionState.Closed);
        private MissionChainData _missionChainData;

        public MissionChainController(int chainId, MissionObjectiveType type, MissionChainData missionChainData)
        {
            ChainId = chainId;
            Type = type;
            MissionData = missionChainData;
        }

        public void SetComplete()
        {
            _state.SetValue(MissionState.Completed, true);
        }

        public void SetInProgress()
        {
            _state.SetValue(MissionState.InProgress, true);
        }

        public void SetFailed()
        {
            _state.SetValue(MissionState.Failed, true);
        }
        
        public void SetPause()
        {
            _state.SetValue(MissionState.Pause, true);
        }


        public void TakeReward()
        {
            _rewardState.SetValue(MissionRewardState.Taken, true);
        }

        public void Save(Dictionary<string, object> data)
        {
            data[nameof(State)] = State.Value;
            data[nameof(RewardState)] = RewardState.Value;
        }

        public void Load(Dictionary<string, object> data)
        {
            var state = (MissionState) System.Convert.ToInt32(data[nameof(State)]);
            _state.SetValue(state, true);

            var rewardState = (MissionRewardState)System.Convert.ToInt32(data[nameof(RewardState)]);
            _rewardState.SetValue(rewardState, true);
        }
    }
}