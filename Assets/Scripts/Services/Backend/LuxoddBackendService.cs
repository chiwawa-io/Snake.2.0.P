using System;
using Core.Enums;
using Core.Events;
using Luxodd.Game.Scripts.Network;
using Services.PlayerData;
using UnityEngine;
using Zenject;

namespace Services.Backend
{
    public class LuxoddBackendService : IBackendService, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly PlayerDataManager _playerDataManager;
        private readonly SignalBus _signalBus;
        
        public LuxoddBackendService(NetworkManager networkManager, PlayerDataManager playerDataManager, SignalBus signalBus)
        {
            _networkManager = networkManager;
            _playerDataManager = playerDataManager;
            _signalBus = signalBus;
        }

        public void Initialize(Action onReady, Action onError)
        {
            _networkManager.WebSocketService.ConnectToServer(onReady, onError);
            _signalBus.Subscribe<ErrorSignal>(HandleError);
            _signalBus.Subscribe<GameOverSignal>(OnGameOver);
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

        public void HandleError(ErrorSignal signal)
        {
            _networkManager.WebSocketService.BackToSystemWithError(signal.Code.ToString(), signal.Message);
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

        private void FinalizeSession(int score)
        {
            _networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(
                0,
                score,
                () => 
                {
                    _playerDataManager.SaveGameSession(score);
                    Exit();
                    
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

        public void Exit()
        {
            _networkManager.WebSocketService.BackToSystem();
        }
    }
}

