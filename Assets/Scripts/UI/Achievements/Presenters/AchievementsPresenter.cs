using Core.Enums;
using Core.Events;
using UI.Achievements.Logic;
using UI.Achievements.Views;
using Zenject;

namespace UI.Achievements.Presenters
{
    public class AchievementsPresenter : IInitializable
    {
        private readonly AchievementService _service;
        private readonly SignalBus _signalBus;
        private readonly AchievementsView _view;
        private int _currentPage;

        public AchievementsPresenter(AchievementService service, AchievementsView view, SignalBus signalBus)
        {
            _service = service;
            _view = view;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _view.OnNextClicked += NextPage;
            _view.OnPrevClicked += PrevPage;
            _view.OnCloseClicked += Close;
        }

        public void Show()
        {
            _currentPage = 0;
            RefreshUI();
        }

        private void NextPage()
        {
            if (_currentPage < _service.GetTotalPages() - 1)
            {
                _currentPage++;
                RefreshUI();
            }
        }

        private void PrevPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            var items = _service.GetPage(_currentPage);
            
            _view.RenderItems(items);

            bool hasNext = _currentPage < _service.GetTotalPages() - 1;
            bool hasPrev = _currentPage > 0;
            
            _view.UpdateNavigation(hasPrev, hasNext, _currentPage + 1, _service.GetTotalPages());
        }

        private void Close()
        {
            _signalBus.Fire(new GameStateChangedSignal { NewState = GameState.MainMenu });
        }
    }
}

