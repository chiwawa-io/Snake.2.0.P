using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Events;
using Cysharp.Threading.Tasks;
using Luxodd.Game.Scripts.Missions;
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.Payloads;
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
        
        public LuxoddBackendService(
            NetworkManager networkManager, 
            PlayerDataManager playerDataManager, 
            SignalBus signalBus, 
            IRngService rngService)
        {
            _networkManager = networkManager;
            _playerDataManager = playerDataManager;
            _signalBus = signalBus;
            _rngService = rngService;
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
        public void TriggerGameOverFlow(int score, Action onRevive)
        {
            _networkManager.WebSocketService.SendSessionOptionContinue((action) => 
            {
                if (action == SessionOptionAction.Continue)
                {
                    onRevive?.Invoke();
                }
                else
                {
                    FinalizeSession(score);
                }
            });
        }
        public void GetGameSessionInfo(Action<SessionInfoPayload> onSuccess, Action<int, string> onError)
        {
            _networkManager.WebSocketCommandHandler.SendGetGameSessionInfoRequestCommand(
                (payload) =>
                {
                    if (payload.SessionType == "sb" && payload.Data != null)
                    {
                        // _rngService.Initialize(payload.Data.Seed);   
                    }
                    onSuccess?.Invoke(payload);
                },
                onError);
        }

        public void SendStrategicBettingResult(List<MissionResultDto> results, Action onSuccess, Action<string> onError)
        {
            
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
                onRevive: () => 
                {
                    _signalBus.Fire(new RevivePlayerSignal());
                }
            );
        }
        

        private void FinalizeSession(int score)
        {
            _networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(
                0,
                score,
                () => 
                {
                    PostGameFlow(score).Forget();
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

        private async UniTask PostGameFlow(int score)
        { 
            _playerDataManager.SaveGameSession(score);
            
            _signalBus.Fire(new GameStateChangedSignal(GameState.PostGameStats)); 
            await UniTask.WaitForSeconds(PostGameFlowWaitTime);
            
            _signalBus.Fire(new GameStateChangedSignal(GameState.Leaderboard));
            await UniTask.WaitForSeconds(PostGameFlowWaitTime);
            
            Exit();
        }

    }
}

