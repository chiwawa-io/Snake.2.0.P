using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Events;
using Cysharp.Threading.Tasks;
using Luxodd.Game.Scripts.Missions;
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.Payloads;
using Newtonsoft.Json.Linq;
using Services.Gameloop;
using Services.PlayerData;
using Services.RNG;
using UnityEngine;
using Zenject;

namespace Services.Backend
{
    public class LuxoddBackendService : IBackendService, IDisposable
    {
        private const int PostGameFlowWaitTime = 5;
        private readonly NetworkManager _networkManager;
        private readonly PlayerDataManager _playerDataManager;
        private readonly SignalBus _signalBus;
        private readonly IRngService _rngService;
        private readonly MissionService _pluginMissionService;
        private StrategicBettingData _currentSbData;
        
        public LuxoddBackendService(
            NetworkManager networkManager, 
            PlayerDataManager playerDataManager, 
            SignalBus signalBus, 
            IRngService rngService,
            MissionService pluginMissionService)
        {
            _networkManager = networkManager;
            _playerDataManager = playerDataManager;
            _signalBus = signalBus;
            _rngService = rngService;
            _pluginMissionService = pluginMissionService;
        }

        public void Initialize(Action onReady, Action onError)
        {
            _networkManager.WebSocketService.ConnectToServer(onReady, onError);
            _networkManager.HealthStatusCheckService.Activate();
            _signalBus.Subscribe<ErrorSignal>(HandleError);
            _signalBus.Subscribe<GameOverSignal>(OnGameOver);
            _signalBus.Subscribe<InactivityTimeOut>(Exit);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<ErrorSignal>(HandleError);
            _signalBus.Unsubscribe<GameOverSignal>(OnGameOver);
        }

        public void StartLevel(Action onSuccess, Action<string> onError)
        {
            _networkManager.WebSocketCommandHandler.SendLevelBeginRequestCommand(
                0, 
                () => onSuccess?.Invoke(),
                (code, msg) => onError?.Invoke($"{code}: {msg}")
            );
        }

        public void GetSeedForRNG(int seed)
        {
            _rngService.Initialize(seed);
        }

        public void HandleError(ErrorSignal signal)
        {
            _networkManager.WebSocketService.BackToSystemWithError(signal.Code.ToString(), signal.Message);
            
        }
        public void TriggerGameOverFlow(int score, int finalLength, GameSessionStats stats, StrategicBettingData sbData, Action onRevive)
        {
            _networkManager.WebSocketService.SendSessionOptionContinue((action) => 
            {
                if (action == SessionOptionAction.Continue)
                {
                    onRevive?.Invoke();
                }
                else
                {
                    FinalizeSession(score, finalLength, stats, sbData);
                }
            });
        }
        public void GetGameSessionInfo(Action<SessionInfoPayload, StrategicBettingData> onSuccess, Action<int, string> onError)
        {
            _networkManager.WebSocketCommandHandler.SendGetGameSessionInfoRequestCommand(
                (payload) =>
                {
                    _currentSbData = null;

                    if (payload.SessionType == "sb" && payload.Data != null)
                    {
                        var token = JToken.FromObject(payload.Data);
                        var sessionInfo = token.ToObject<GameSessionInfoData>();

                        if (!Enum.TryParse(sessionInfo.LevelDifficulty, true, out DifficultyLevel difficulty))
                            difficulty = DifficultyLevel.Easy;

                        var missions = token["missions"]?.ToObject<List<MissionBettingInfo>>() 
                                       ?? new List<MissionBettingInfo>();

                        _currentSbData = new StrategicBettingData
                        {
                            LevelId = sessionInfo.LevelId,
                            LevelDifficulty = difficulty,
                            Missions = missions
                        };

                        int seed = 12345; 

                        if (token["seed"] != null)
                        {
                            seed = token["seed"].Value<int>();
                        }
                        
                        _rngService.Initialize(seed);
                        
                        _pluginMissionService.PrepareSelectedMissionList(_currentSbData);
                    }

                    onSuccess?.Invoke(payload, _currentSbData);
                },
                onError);
        }

        public void SendStrategicBettingResult(List<MissionResultDto> results, Action onSuccess, Action<int,string> onError)
        {
            _networkManager.WebSocketCommandHandler.SendStrategicBettingResultRequest(
                results,
                onSuccess, 
                onError
            );
        }
        public void Exit()
        {
            _networkManager.HealthStatusCheckService.Deactivate();
            _networkManager.WebSocketService.BackToSystem();
        }

        private void OnGameOver(GameOverSignal signal)
        {
            TriggerGameOverFlow(
                signal.FinalScore,
                signal.FinalLength,
                signal.Stats,
                _currentSbData, 
                onRevive: () => 
                {
                    _signalBus.Fire(new RevivePlayerSignal());
                }
            );
        }
        

        private void FinalizeSession(int score, int finalLength, GameSessionStats stats, StrategicBettingData sbData)
        {
            _networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(
                0,
                score,
                () => 
                {
                    PostGameFlow(score, finalLength, stats, sbData).Forget();
                    // Restart option will be needed in the future
                    // _networkManager.WebSocketService.SendSessionOptionRestart((action) => 
                    // {
                    //     if (action == SessionOptionAction.Restart)
                    //     {
                    //         onFinalize?.Invoke(); 
                    //     }
                    //     else
                    //     {
                    //         _networkManager.WebSocketService.BackToSystem();
                    //     }
                    // });
                },
                (code, msg) => Debug.LogError($"Save Failed: {msg}")
            );
        }

        private async UniTask PostGameFlow(int score, int finalLength, GameSessionStats stats, StrategicBettingData sbData)
        { 
            _playerDataManager.SaveGameSession(score);

            if (sbData != null && sbData.Missions != null)
            {
                var results = new List<MissionResultDto>();

                foreach (var mission in sbData.Missions)
                {
                    var states = _pluginMissionService.GetMissionStatesByMissionId(mission.MissionId);
                    
                    bool isWin = states.Contains(MissionState.Completed);

                    results.Add(new MissionResultDto
                    {
                        MissionId = mission.MissionId,
                        Outcome = isWin ? "win" : "loss"
                    });
                }

                SendStrategicBettingResult(results, 
                    onSuccess: () => Debug.Log("[SB] Results verified by server."),
                    onError: (code, msg) => Debug.LogError($"[SB] Reporting Failed: {msg}")
                );
            }
            _signalBus.Fire(new GameStateChangedSignal(GameState.PostGameStats)); 
            await UniTask.WaitForSeconds(PostGameFlowWaitTime);
            
            _signalBus.Fire(new GameStateChangedSignal(GameState.Leaderboard));
            await UniTask.WaitForSeconds(PostGameFlowWaitTime);
            
            Exit();
        }

    }
}

