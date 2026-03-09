using System;
using Core.Enums;
using Core.Events;
using UI.Achievements.Views;
using UI.Game.Views;
using UI.Leaderboard.Views;
using UI.MainMenu.Views;
using UI.MissionCompletion.Views;
using UI.Other;
using Zenject;

namespace UI.Global
{
    public class UIManager : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly MainMenuView _mainMenu;
        private readonly GameView _hud;
        private readonly StatsView _statsView;
        private readonly LeaderboardView _leaderboard;
        private readonly AchievementsView _achievements;
        private readonly MissionCompletionView _missionCompletionView;
        private readonly BaseView _loadingView;
        private readonly ErrorView _errorView; 

        public UIManager(
            SignalBus signalBus, 
            MainMenuView mainMenu,
            GameView hud,
            StatsView statsView,
            LeaderboardView leaderboard,
            AchievementsView achievements,
            [Inject(Id = "Loading")] BaseView loading,
            MissionCompletionView missionCompletionView,
            ErrorView errorView)
        {
            _signalBus = signalBus;
            _mainMenu = mainMenu;
            _hud = hud;
            _statsView = statsView;
            _leaderboard = leaderboard;
            _achievements = achievements;
            _loadingView = loading;
            _missionCompletionView = missionCompletionView;
            _errorView = errorView;
        }

        public void Initialize()
        {
            HideAll();
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameStateChangedSignal>(OnStateChanged);
        }

        private void OnStateChanged(GameStateChangedSignal signal)
        {
            HideAll();
            
            switch (signal.NewState)
            {
                case GameState.Loading:
                case GameState.LevelLoading: _loadingView.Show(); break;
                case GameState.MainMenu: _mainMenu.Show(); break;
                case GameState.InGame: _hud.Show(); break;
                case GameState.Leaderboard: _leaderboard.Show(); break;
                case GameState.Achievements: _achievements.Show(); break;
                case GameState.PostGameStats: _statsView.Show(); break;
                case GameState.MissionCompletion: _missionCompletionView.Show(); break;
                case GameState.Error: _errorView.Show(); break;
            }
        }

        private void HideAll()
        {
            _mainMenu.Hide();
            _hud.Hide();
            _statsView.Hide();
            _leaderboard.Hide();
            _achievements.Hide();
            _loadingView.Hide();
            _missionCompletionView.Hide();
            _errorView.Hide();
        }
    }
}