using UnityEngine;
using Zenject;

namespace Gameplay.Snake{
    public class SnakeVisualUpdater : MonoBehaviour
    {
        [Inject] private SnakeModel _model;
        [Inject] private SnakeView _view;

        private void Update()
        {
            if (_model != null)
                _view.UpdateVisuals(Time.time);
        }
    }
}
