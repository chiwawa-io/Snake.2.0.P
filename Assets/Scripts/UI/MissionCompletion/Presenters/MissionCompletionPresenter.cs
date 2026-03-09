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

        public MissionCompletionPresenter(
            SignalBus signalBus, 
            MissionCompletionView view, 
            IBackendService backendService,
            MissionService pluginMissionService) 
        {
            _signalBus = signalBus;
            _view = view;
            _backendService = backendService;
            _pluginMissionService = pluginMissionService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
            _signalBus.Subscribe<LevelCompletedSignal>(StatusUpdate);
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

        private void StatusUpdate()
        {
            _view.DisplayWonTitle();
        }

        private void ShowMissionResults()
        {
            var sbData = _backendService.GetCachedGameSessionInfo();
            if (sbData == null || sbData.Missions == null) return;

            var uiDataList = new List<MissionCompletionUIData>();

            foreach (var mission in sbData.Missions)
            {
                var states = _pluginMissionService.GetMissionStatesByMissionId(mission.MissionId);
                bool isWin = states.Contains(MissionState.Completed);

                var serverMissionDef = _backendService.GetMissionDefinition(mission.MissionId);
                
                string name = serverMissionDef != null ? serverMissionDef.Name : "Unknown Mission";
                string desc = serverMissionDef != null ? serverMissionDef.Description : "";

                uiDataList.Add(new MissionCompletionUIData 
                {
                    MissionName = name,
                    Description = desc,
                    IsCompleted = isWin
                });
            }

            _view.DisplayMissions(uiDataList);
            _view.Show();
        }
    }
}