using System;
using System.Collections.Generic;
using System.Linq;
using Luxodd.Game.HelpersAndUtils.Utils;
using Luxodd.Game.Scripts.HelpersAndUtils.Missions;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public interface IMissionInfo
    {
        IIntReadOnlyProperty ProgressValue { get; }
        IReadOnlyCollection<IMissionChainInfo> MissionChainInfoList { get; }
        IMissionChainInfo GetMissionChainInfo(int chainId);
        MissionObjectiveType Type { get; }
        string Description { get; }
        int MissionRewardBundleId { get; }
        float TargetValue { get; }
    }


    public interface IMissionController : IMissionInfo
    {
        void Start();
        void SetHardness(int value);
        void Stop();
    }

    public abstract class BaseMissionController : IMissionController
    {
        private readonly MissionDefinition _missionData;
        private readonly List<IMissionChainController> _missionChainControllerList = new List<IMissionChainController>();
        private readonly Dictionary<Type, Delegate> _subscribedEvents = new Dictionary<Type, Delegate>();
        public IReadOnlyCollection<IMissionChainInfo> MissionChainInfoList => _missionChainControllerList;
        public IIntReadOnlyProperty ProgressValue => _progressValue;
        public float TargetValue => _targetValue;
        public abstract MissionObjectiveType Type { get; }
        public string Description => _missionData.DescriptionKey;
        public int MissionRewardBundleId => _missionData.MissionRewardBundleId;
        public IMissionChainInfo GetMissionChainInfo(int chainId)
            => _missionChainControllerList.FirstOrDefault(c => c.ChainId == chainId);

        public void SetHardness(int value) => CurrentHardness = value;

        protected IntProperty _progressValue { get; } = new IntProperty();
        protected int CurrentChainIndex { get; private set; }
        protected int CurrentHardness { get; private set; }
        protected float _targetValue { get; private set; }

        protected IMissionChainController CurrentChain => _missionChainControllerList[CurrentChainIndex];
        protected bool IsLastChain => CurrentChainIndex >= _missionData.ChainData.Count - 1;

        private IMissionChainController CreateChainController(int id, MissionChainData data)
            => new MissionChainController(id, Type, data);


        public void Start()
        {
            if (IsLastChain && CurrentChain.State.Value == MissionState.Completed)
                return;

            _targetValue = CalculateMissionHardness(CurrentHardness);
            SubscribeToEvents();
        }

        public void CompleteMissionManually()
        {
            CurrentChain.SetComplete();
            _progressValue.SetValue((int)_targetValue, true);
            UnsubscribeAllEvents();
        }

        public void Stop() => UnsubscribeAllEvents();

        protected virtual void SubscribeToEvents()
        {
            EventAggregator.Subscribe<MissionProgressEvent>(OnMissionProgressHandler);
        }

        protected virtual void UnsubscribeFromEvent()
        {
            EventAggregator.Unsubscribe<MissionProgressEvent>(OnMissionProgressHandler);
        }

        protected virtual void UnsubscribeAllEvents()
        {
            EventAggregator.Unsubscribe<MissionProgressEvent>(OnMissionProgressHandler);

        }

        protected void CheckChainForCompletion()
        {
            if (_progressValue.Value >= _targetValue)
                CompleteChain();
        }

        protected BaseMissionController(MissionDefinition missionData)
        {
            _missionData = missionData ?? throw new ArgumentNullException(nameof(missionData));
            PrepareChainControllers();
        }

        private void PrepareChainControllers()
        {
            for (int i = 0; i < _missionData.ChainData.Count; i++)
            {
                var chainController = CreateChainController(i, _missionData.ChainData[i]);
                _missionChainControllerList.Add(chainController);

                if (i == 0)
                    chainController.SetInProgress();
            }
        }

        private float CalculateMissionHardness(int difficulty)
            => Mathf.Lerp(CurrentChain.MissionData.MinValue, CurrentChain.MissionData.MaxValue, Mathf.Clamp(difficulty, 1, 100) / 100f);

        private void OnMissionProgressHandler(object sender, MissionProgressEvent eventData)
        {
            if (eventData.Type != Type) return;

            _progressValue.SetValue(eventData.Value, true);
            CheckChainForCompletion();
        }

        private void CompleteChain()
        {
            CurrentChain.SetComplete();
            CurrentChainIndex++;

            if (CurrentChainIndex >= _missionData.ChainData.Count)
            {
                CurrentChainIndex = _missionData.ChainData.Count - 1;
                UnsubscribeAllEvents();
            }
            else
            {
                CurrentChain.SetInProgress();
            }
        }

    }

    public class CompleteLevelMissionController : BaseMissionController
    {
        public CompleteLevelMissionController(MissionDefinition missionData) : base(missionData) { }

        public override MissionObjectiveType Type => MissionObjectiveType.FinishLevel;

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventAggregator.Subscribe<FinishLevelEvent>(OnLevelCompletionEventHandler);
        }


        private void OnLevelCompletionEventHandler(object sender, FinishLevelEvent eventData)
        {
            CurrentChain.SetComplete();
        }
    }
}