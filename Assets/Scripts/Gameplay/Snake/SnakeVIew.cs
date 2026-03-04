using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Gameplay.Snake
{
    public class SnakeView : MonoBehaviour
    {
        private const float FlickerPulseSpeed = 10f;

        [Header("Visuals")] 
        [SerializeField] private GameObject _snakeHeadVisual;
        [SerializeField] private GameObject _snakeTailVisual;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private GameObject _boomEffect;
        [SerializeField] private float _visualLerpSpeed = 8f;

        private SpriteRenderer _headRenderer;
        private SpriteRenderer _tailRenderer;
        private SnakeModel _model;
        private Color _originalColor;
        private Color _flickerColor;

        [Inject]
        public void Construct(SnakeModel model)
        {
            _model = model;
        }

        public void ToggleVisuals(bool isActive)
        {
            _snakeHeadVisual.SetActive(isActive);
            _snakeTailVisual.SetActive(isActive);
            _lineRenderer.gameObject.SetActive(isActive);
        }

        public void PlayBoomEffect()
        {
            if (_boomEffect)
            {
                _boomEffect.transform.position = _snakeHeadVisual.transform.position;
                _boomEffect.SetActive(false);
                _boomEffect.SetActive(true);
            }
        }

        public void UpdateVisuals(float interpolationFactor)
        {
            if (_model.Body.Count < 2) return;

            List<Vector3> visualPoints = new List<Vector3>();

            Vector2 visualHeadPos = Vector2.Lerp(_model.Body[1], _model.Body[0], interpolationFactor);
            _snakeHeadVisual.transform.position = visualHeadPos;
            visualPoints.Add(visualHeadPos);
            UpdateRotation(_snakeHeadVisual.transform, _model.Body[0], _model.Body[1]);

            for (int i = 1; i < _model.Body.Count - 1; i++)
            {
                Vector2 prevPos = _model.Body[i + 1];
                Vector2 currentPos = _model.Body[i];
                visualPoints.Add(Vector2.Lerp(prevPos, currentPos, interpolationFactor));
            }

            Vector2 tailCurrentPos = _model.Body.Last();
            Vector2 visualTailPos;

            float dist = Vector2.Distance(_model.LastTailPosition, tailCurrentPos);

            if (dist > 1.5f)
            {
                visualTailPos = tailCurrentPos;
            }
            else
            {
                visualTailPos = Vector2.Lerp(_model.LastTailPosition, tailCurrentPos, interpolationFactor);
            }

            _snakeTailVisual.transform.position = visualTailPos;
            visualPoints.Add(visualTailPos);

            if (_model.Body.Count >= 2)
            {
                UpdateRotation(_snakeTailVisual.transform, _model.Body[^2], _model.Body.Last());
            }

            _lineRenderer.positionCount = visualPoints.Count;
            _lineRenderer.SetPositions(visualPoints.ToArray());

            ApplyFlickerEffect();
        }

        private void Awake()
        {
            _headRenderer = _snakeHeadVisual.GetComponent<SpriteRenderer>();
            _tailRenderer = _snakeTailVisual.GetComponent<SpriteRenderer>();
            
            _originalColor = _headRenderer.color;
            _flickerColor = Color.white;
        }

        private void UpdateRotation(Transform target, Vector2 to, Vector2 from)
        {
            Vector2 dir = to - from;
            if (dir == Vector2.zero) return;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, angle - 90);
            target.rotation = Quaternion.Slerp(target.rotation, targetRot, _visualLerpSpeed * Time.deltaTime);
        }

        private void ApplyFlickerEffect()
        {
            if (_model.IsInvulnerable)
            {
                float t = (Mathf.Sin(Time.time * FlickerPulseSpeed) + 1f) / 2f;
                Color currentColor = Color.Lerp(_originalColor, _flickerColor, t);

                SetSnakeColor(currentColor);
            }
            else
            {
                if (_headRenderer.color != _originalColor)
                {
                    SetSnakeColor(_originalColor);
                }
            }
        }

        private void SetSnakeColor(Color color)
        {
            _headRenderer.color = color;
            _tailRenderer.color = color;
            
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }
    }
}