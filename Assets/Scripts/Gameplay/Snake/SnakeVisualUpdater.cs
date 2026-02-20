using UnityEngine;
using Zenject;

namespace Gameplay.Snake
{
    public class SnakeVisualUpdater : MonoBehaviour
    {
        [Inject] private SnakeView _view;
        [Inject] private SnakeController _controller;

        private void Update()
        {
            var interpolationFactor = _controller.InterpolationFactor;

            _view.UpdateVisuals(interpolationFactor);
        }
    }
}
