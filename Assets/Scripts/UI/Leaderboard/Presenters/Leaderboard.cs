using System.Linq;
using Core.Enums;
using Core.Events;
using Luxodd.Game.Scripts.Game.Leaderboard;
using UnityEngine;
using Services.Backend;
using UI.Leaderboard.Views;
using Zenject;

namespace UI.Leaderboard.Presenters
{
    public class Leaderboard : MonoBehaviour
    {
        [SerializeField] private LeaderboardView leaderboardView;

        private NetworkManager _networkManager;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(NetworkManager networkManager, SignalBus signalBus)
        {
            _networkManager = networkManager;
            _signalBus = signalBus;
        }

        public void FetchLeaderboard()
        {
            _networkManager.WebSocketCommandHandler.SendLeaderboardRequestCommand(
                OnSuccess,
                (code, msg) => Debug.LogError($"Leaderboard Error: {msg}")
            );
        }

        public void OnReturnClicked()
        {
            _signalBus.Fire(new GameStateChangedSignal(GameState.MainMenu));
        }

        private void OnEnable()
        {
            FetchLeaderboard();
        }

        private void Start()
        {
            _signalBus.Subscribe<InactivityTimerSignal>(OnTimerUpdate);
        }

        private void OnSuccess(LeaderboardDataResponse response)
        {
            if (response.Leaderboard == null) return;

            var list = response.Leaderboard.ToList();
            leaderboardView.Populate(list);
        }

        private void OnTimerUpdate(InactivityTimerSignal signal)
        {
            leaderboardView.UpdateTimer(signal.SecondsLeft.ToString());
        }
    }
}