using System;
using UnityEngine;
using UI.Global;

namespace UI.Game.Views
{
    public class MobileInputView : BaseView
    {
        [SerializeField] private DirectionalButton _upButton;
        [SerializeField] private DirectionalButton _downButton;
        [SerializeField] private DirectionalButton _leftButton;
        [SerializeField] private DirectionalButton _rightButton;

        public event Action<Vector2Int> OnDirectionPressed;

        private void OnEnable()
        {
            if (_upButton != null) _upButton.OnPressed += HandlePressed;
            if (_downButton != null) _downButton.OnPressed += HandlePressed;
            if (_leftButton != null) _leftButton.OnPressed += HandlePressed;
            if (_rightButton != null) _rightButton.OnPressed += HandlePressed;
        }
        
        private void OnDisable()
        {
            if (_upButton != null) _upButton.OnPressed -= HandlePressed;
            if (_downButton != null) _downButton.OnPressed -= HandlePressed;
            if (_leftButton != null) _leftButton.OnPressed -= HandlePressed;
            if (_rightButton != null) _rightButton.OnPressed -= HandlePressed;
        }

        private void HandlePressed(Vector2Int dir) => OnDirectionPressed?.Invoke(dir);
    }
}
