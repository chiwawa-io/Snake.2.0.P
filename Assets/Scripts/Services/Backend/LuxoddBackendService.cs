using System;
using Core.Events;
using Luxodd.Game.Scripts.Network;
using Services.PlayerData;
using UnityEngine;
using Zenject;

namespace Services.Backend
{
    public class LuxoddBackendService : IBackendService
    {
        private readonly NetworkManager _networkManager;
        private readonly PlayerDataManager _playerDataManager;
        private SignalBus _signalBus;
        
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

        public void TriggerGameOverFlow(int score, Action onRevive, Action onFinalize)
        {
            _networkManager.WebSocketService.SendSessionOptionContinue((action) => 
            {
                if (action == SessionOptionAction.Continue)
                {
                    onRevive?.Invoke();
                }
                else
                {
                    FinalizeSession(score, onFinalize);
                }
            });
        }

        private void FinalizeSession(int score, Action onFinalize)
        {
            _networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(
                0,
                score,
                () => 
                {
                    _playerDataManager.SaveGameSession(score);
                    _networkManager.WebSocketService.BackToSystem();
                    
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
    }
}

