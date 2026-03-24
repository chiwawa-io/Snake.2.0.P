using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Events;
using Luxodd.Game.Scripts.Missions;
using Services.Backend;
using UI.MissionCompletion.Data;
using UI.MissionCompletion.Views;
using Zenject;

namespace UI.MissionCompletion.Presenters
{
    public class MissionCompletionPresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly MissionCompletionView _view;
        private readonly IBackendService _backendService;
        private readonly MissionService _pluginMissionService;
        private readonly MissionDataBase _missionDatabase; 

        public MissionCompletionPresenter(
            SignalBus signalBus, 
            MissionCompletionView view, 
            IBackendService backendService,
            MissionService pluginMissionService,
            MissionDataBase missionDatabase) 
        {
            _signalBus = signalBus;
            _view = view;
            _backendService = backendService;
            _pluginMissionService = pluginMissionService;
            _missionDatabase = missionDatabase;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GameStateChangedSignal>(OnStateChanged);
        }

        private void OnStateChanged(GameStateChangedSignal signal)
        {
            if (signal.NewState == GameState.MissionCompletion)
            {
                ShowMissionResults();
            }
        }

        private void ShowMissionResults()
        {
            var sbData = _backendService.GetCachedGameSessionInfo();
            if (sbData == null || sbData.Missions == null) return;

            var uiDataList = new List<MissionCompletionUIData>();

            foreach (var mission in sbData.Missions)
            {
                // 1. Get status from the plugin's internal tracker
                var states = _pluginMissionService.GetMissionStatesByMissionId(mission.MissionId);
                bool isWin = states.Contains(MissionState.Completed);

                // 2. Fetch the local ScriptableObject using the ID provided by the server
                var missionDef = _missionDatabase.ProvideMissionDataById(mission.MissionId);
                
                string name = missionDef != null ? missionDef.Name : "Unknown Mission";
                string desc = missionDef != null ? missionDef.Description : "";
                MissionType type = missionDef != null ? missionDef.Type : MissionType.Primary;

                uiDataList.Add(new MissionCompletionUIData 
                {
                    MissionName = name,
                    Description = desc,
                    Type = type,
                    IsCompleted = isWin,
                    Payout = isWin ? (mission.Bet * mission.Ratio) : 0f
                });
            }

            _view.DisplayMissions(uiDataList);
            _view.Show();
        }
    }
}