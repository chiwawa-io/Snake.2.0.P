using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Game.Views
{
    public class DirectionalButton : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Vector2Int _direction;
        
        public event Action<Vector2Int> OnPressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPressed?.Invoke(_direction);
        }
    }
}
