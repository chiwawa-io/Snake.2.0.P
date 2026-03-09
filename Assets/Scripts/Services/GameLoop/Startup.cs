using Core.Enums;
using Core.Events;
using Services.Backend;
using Services.PlayerData; 
using UnityEngine;
using Zenject;

namespace Services.Gameloop
{
    public class Startup : IInitializable
    {
        private readonly SignalBus _signalBus;
        private readonly IBackendService _backendService;
        private readonly PlayerDataManager _playerDataManager;

        public Startup(
            SignalBus signalBus,
            IBackendService backendService,
            PlayerDataManager playerDataManager)
        {
            _signalBus = signalBus;
            _backendService = backendService;
            _playerDataManager = playerDataManager;
        }

        public void Initialize()
        {
            _signalBus.Fire(new GameStateChangedSignal(GameState.Loading));

            _backendService.Initialize(OnConnectionSuccess, OnConnectionError);
        }

        private void OnConnectionSuccess()
        {
            _backendService.FetchMissionDefinitions(
            onSuccess: () => 
            {
                _playerDataManager.LoadData();
            },
            onError: (error) => 
            {
                _playerDataManager.LoadData(); 
            }
        );
        }

        private void OnConnectionError()
        {
            var error = "ConnectionFailed";
            Debug.LogWarning(error);
            _signalBus.Fire(new ErrorSignal(500, error));
        }
    }
}