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
#if UNITY_EDITOR
        private const bool UseMockDataInEditor = true; 
        private const int MockSeed = 12345;
        private const int MockTargetLength = 24; 
        private const DifficultyLevel MockDifficulty = DifficultyLevel.Hard;
        private const string MockPrimaryMissionId = "mission_main_1"; 
#endif
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
            _networkManager.WebSocketService.ConnectToServer(() => 
            {
                // Fetch session info immediately after connection to populate SB cache for MainMenu
                GetGameSessionInfo(
                    success => onReady?.Invoke(), 
                    (code, msg) => onError?.Invoke()
                );
            }, onError);
            _networkManager.HealthStatusCheckService.Activate();
            _signalBus.Subscribe<ErrorSignal>(HandleError);
            _signalBus.Subscribe<GameOverSignal>(OnGameOver);
            _signalBus.Subscribe<InactivityTimeOut>(Exit);
            _signalBus.Subscribe<UpdateMissionProgressSignal>(OnUpdateMissionProgress);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<ErrorSignal>(HandleError);
            _signalBus.TryUnsubscribe<GameOverSignal>(OnGameOver);
            _signalBus.TryUnsubscribe<InactivityTimeOut>(Exit);
            _signalBus.TryUnsubscribe<UpdateMissionProgressSignal>(OnUpdateMissionProgress);
        }

        public void StartLevel(Action onSuccess, Action<string> onError)
        {
#if UNITY_EDITOR
            if (UseMockDataInEditor)
            {
                UniTask.Delay(TimeSpan.FromSeconds(0.2f)).ContinueWith(() => onSuccess?.Invoke());
                return;
            }
#endif
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
#if UNITY_EDITOR
            if (UseMockDataInEditor)
            {
                Debug.LogWarning("Finalizing session.");
                FinalizeSession(score, finalLength, stats, sbData);
                return;
            }
#endif
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

        public StrategicBettingData GetCachedGameSessionInfo()
        {
            if (_currentSbData == null)
                GetGameSessionInfo
                (
                    data => _currentSbData = data,
                    (code, msg) => Debug.LogWarning($"{code} + {msg}")
                );
            return _currentSbData;
        }

        public void GetGameSessionInfo(Action<StrategicBettingData> onSuccess, Action<int, string> onError)
        {
            GetGameSessionInfo((payload, sbData) => onSuccess?.Invoke(sbData), onError);
        }

        public void GetGameSessionInfo(Action<SessionInfoPayload, StrategicBettingData> onSuccess, Action<int, string> onError)
        {
#if UNITY_EDITOR
            if (UseMockDataInEditor)
            {
                Debug.LogWarning("Generating Fake Strategic Betting Payload");
                
                _currentSbData = new StrategicBettingData
                {
                    LevelId = 1,
                    LevelDifficulty = MockDifficulty,
                    Missions = new List<MissionBettingInfo>
                    {
                        new MissionBettingInfo
                        {
                            MissionId = MockPrimaryMissionId, 
                            Bet = 10f,
                            CalculatedHardness = 85f,
                            Value = MockTargetLength
                        }
                    }
                };

                var mockPayload = new SessionInfoPayload
                {
                    SessionType = "sb",
                    Data = null 
                };

                _rngService.Initialize(MockSeed);
                _pluginMissionService.PrepareSelectedMissionList(_currentSbData);
                
                UniTask.Delay(TimeSpan.FromSeconds(0.5f)).ContinueWith(() => onSuccess?.Invoke(mockPayload, _currentSbData));
                return;
            }
#endif
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
#if UNITY_EDITOR
            if (UseMockDataInEditor)
            {
                Debug.LogWarning("Intercepted Results sent to fake server");
                UniTask.Delay(TimeSpan.FromSeconds(0.3f)).ContinueWith(() => onSuccess?.Invoke());
                return;
            }
#endif
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

        private void OnUpdateMissionProgress(UpdateMissionProgressSignal signal)
        {
            if (_currentSbData == null || _currentSbData.Missions == null) return;
            
            foreach (var mission in _currentSbData.Missions)
            {
                // _pluginMissionService.UpdateProgress(mission.MissionId, signal.CurrentValue);
            }
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
#if UNITY_EDITOR
            if (UseMockDataInEditor)
            {
                PostGameFlow(score, finalLength, stats, sbData).Forget();
                return;
            }
#endif
            _networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(
                0,
                score,
                () => 
                {
                    PostGameFlow(score, finalLength, stats, sbData).Forget();
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
                
                _signalBus.Fire(new GameStateChangedSignal(GameState.MissionCompletion));
                await UniTask.WaitForSeconds(PostGameFlowWaitTime);
            }

            _signalBus.Fire(new GameStateChangedSignal(GameState.PostGameStats)); 
            await UniTask.WaitForSeconds(PostGameFlowWaitTime);
            
            _signalBus.Fire(new GameStateChangedSignal(GameState.Leaderboard));
            await UniTask.WaitForSeconds(PostGameFlowWaitTime);
            
            Exit();
        }
    }
}