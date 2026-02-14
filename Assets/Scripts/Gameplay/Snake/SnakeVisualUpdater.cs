using UnityEngine;
using Zenject;

namespace Gameplay.Snake{
    public class SnakeVisualUpdater : MonoBehaviour
    {
        [Inject] private SnakeView _view;
        [Inject] private SnakeGameController _controller;

        private void Update()
        {
            float interpolationFactor = _controller.InterpolationFactor;

            _view.UpdateVisuals(interpolationFactor);
        }
    }
}
