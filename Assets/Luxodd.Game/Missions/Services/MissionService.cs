using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public interface IMissionService
    {
        IReadOnlyCollection<IMissionInfo> AllMissionInfo { get; }
        IMissionInfo GetMissionInfo(MissionObjectiveType missionType);
        void TakeReward(MissionObjectiveType type, int chainId);
    }

    public class MissionService : MonoBehaviour, IMissionService
    {
        public IReadOnlyCollection<IMissionInfo> AllMissionInfo => _allMissionControllers;

        [SerializeField] private MissionRepository _missionRepository;
        [SerializeField] private MissionControllerProvider _missionControllerProvider;
        [SerializeField] private NotificationController _notificationController;
        [SerializeField] private RewardService _rewardService;

        [field: SerializeField] private int _hardness;
        private Action _notificationValueStateChange;

        private readonly List<IMissionController> _allMissionControllers = new();
        private readonly Dictionary<MissionObjectiveType, IMissionController> _missionControllersDictionary = new();
        private readonly Dictionary<string, List<IMissionChainInfo>> _missionChainsByMissionId = new();
        private readonly Dictionary<MissionObjectiveType, int> _hardnessByMissionType = new();


        public IMissionInfo GetMissionInfo(MissionObjectiveType missionType)
            => _missionControllersDictionary.TryGetValue(missionType, out var controller) ? controller : null;

        public void TakeReward(MissionObjectiveType type, int chainId)
        {
            // TODO: Implement the reward system.
        }

        public void ProvideDependencies(Action receiveReward)
        {
            // TODO: Implement the reward system.
        }


        public List<MissionState> GetMissionStatesByMissionId(string missionId)
        {
            return _missionChainsByMissionId.TryGetValue(missionId, out var chains)
                ? chains.Select(c => c.State.Value).ToList()
                : new List<MissionState>();
        }

        public List<MissionState> GetMissionStatesOfFirstMission()
        {
            var firstMissionData = _missionRepository.MissionDataList.FirstOrDefault();
            return firstMissionData != null
                && _missionChainsByMissionId.TryGetValue(firstMissionData.ID, out var chains)
                ? chains.Select(c => c.State.Value).ToList()
                : new List<MissionState>();
        }

        public void CompleteMissionById(string missionId)
        {
            if (!_missionChainsByMissionId.TryGetValue(missionId, out var chainList))
            {
                Debug.LogWarning($"Mission with ID {missionId} not found.");
                return;
            }

            foreach (var chain in chainList.Where(c => c.State.Value != MissionState.Completed))
            {
                if (_allMissionControllers.FirstOrDefault(q => q.MissionChainInfoList.Contains(chain)) is BaseMissionController baseController)
                {
                    baseController.CompleteMissionManually();
                }
            }
        }

        public int GetHardnessByType(MissionObjectiveType type)
            => _hardnessByMissionType.TryGetValue(type, out var hardness) ? hardness : 0;


        public void PrepareSelectedMissionList(StrategicBettingData strategicBettingData)
        {
            PrepareMissions(strategicBettingData);
        }


        private void PrepareMissions(StrategicBettingData strategicBettingData)
        {
            _hardnessByMissionType.Clear();

            foreach (var missionData in _missionRepository.MissionDataList)
            {
                var missionController = CreateMissionController(missionData);

                _allMissionControllers.Add(missionController);
                _missionControllersDictionary[missionController.Type] = missionController;

                if (_hardnessByMissionType.TryGetValue(missionController.Type, out int hardness))
                {
                    if (_hardness > 0) {
                        hardness = _hardness;
                    }
                }

                missionController.SetHardness(_hardness);
                missionController.Start();

                _missionChainsByMissionId[missionData.ID] =
                    missionController.MissionChainInfoList.ToList();

            }

            PrepareNotificationControllerData();
        }




        private IMissionController CreateMissionController(MissionDefinition data)
            => _missionControllerProvider.Provide(data.Type, data);

        private void PrepareNotificationControllerData()
        {
            foreach (var missionController in _allMissionControllers)
            {
                foreach (var chain in missionController.MissionChainInfoList)
                {
                    chain.RewardState.AddListener(OnMissionChangeRewardStateListener);
                    chain.State.AddListener(state => OnMissionChainStateChanged(state, missionController));
                }
            }
        }

        private void OnMissionChainStateChanged(MissionState state, IMissionController controller)
        {
            // Called when the mission chain state changes.
            // Use this method to react to mission progress updates
        }

        private void OnMissionChangeRewardStateListener(MissionRewardState rewardState)
        {
            _notificationValueStateChange?.Invoke();
        }

        private void OnRewardPopupHideHandler()
        {
            // TODO: Implement the reward system.
        }
    }
}
