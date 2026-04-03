using System;
using Services.PlayerInput;
using UI.Game.Views;
using UnityEngine;
using Zenject;

namespace UI.Game.Presenters
{
    public class MobileInputPresenter : IInitializable, IDisposable
    {
        private readonly MobileInputView _view;
        private readonly IInputProvider _inputProvider;

        public MobileInputPresenter(MobileInputView view, [InjectOptional] IInputProvider inputProvider)
        {
            _view = view;
            _inputProvider = inputProvider;
        }

        public void Initialize()
        {
            _view.OnDirectionPressed += OnDirectionPressed;
        }

        public void Dispose()
        {
            _view.OnDirectionPressed -= OnDirectionPressed;
        }

        private void OnDirectionPressed(Vector2Int direction)
        {
            if (_inputProvider is MobileInputProvider mobileProvider)
            {
                mobileProvider.HandleInput(direction);
            }
        }
    }
}
