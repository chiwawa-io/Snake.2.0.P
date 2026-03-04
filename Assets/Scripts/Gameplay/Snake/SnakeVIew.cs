using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Gameplay.Snake
{
    public class SnakeView : MonoBehaviour
    {
        private const float FlickerSpeed = 2.5f;

        [Header("Visuals")] 
        [SerializeField] private GameObject _snakeHeadVisual;
        [SerializeField] private GameObject _snakeTailVisual;
        [SerializeField] private LineRenderer _lineRenderer;
        
        [Header("Effects")]
        [SerializeField] private GameObject _boomEffect;
        [SerializeField] private GameObject _preciousGemEffect;
        [SerializeField] private GameObject _gemEffect;
        [SerializeField] private GameObject _speedUpEffect;
        [SerializeField] private GameObject _invulnerabilityEffect;

        [SerializeField] private float _visualLerpSpeed = 8f;

        private SpriteRenderer _headRenderer;
        private SnakeModel _model;
        private Color _originalColor;

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

        public void PlayBoomEffect()
        {
            if (_boomEffect)
            {
                _boomEffect.transform.position = _snakeHeadVisual.transform.position;
                _boomEffect.SetActive(false);
                _boomEffect.SetActive(true);
            }
        }

        public void PlayGemEffect()
        {
            if (_gemEffect)
            {
                _gemEffect.transform.position = _snakeHeadVisual.transform.position;
                _gemEffect.SetActive(false);
                _gemEffect.SetActive(true);
            }
        }

        public void PlayPreciousGemEffect()
        {
            if (_preciousGemEffect)
            {
                _preciousGemEffect.transform.position = _snakeHeadVisual.transform.position;
                _preciousGemEffect.SetActive(false);
                _preciousGemEffect.SetActive(true);
            }
        }

        public void PlaySpeedUpEffect()
        {
            if (_speedUpEffect)
            {
                _speedUpEffect.transform.position = _snakeHeadVisual.transform.position;
                _speedUpEffect.SetActive(false);
                _speedUpEffect.SetActive(true);
            }
        }

        public void PlayInvulnerabilityEffect()
        {
            if (_invulnerabilityEffect)
            {
                _invulnerabilityEffect.transform.position = _snakeHeadVisual.transform.position;
                _invulnerabilityEffect.SetActive(false);
                _invulnerabilityEffect.SetActive(true);
            }
        }
        
        private void Awake()
        {
            _headRenderer = _snakeHeadVisual.GetComponent<SpriteRenderer>();
            _originalColor = _headRenderer.color;
        }

        private void UpdateRotation(Transform target, Vector2 to, Vector2 from)
        {
            var dir = to - from;
            if (dir == Vector2.zero) return;

            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, angle - 90);
            target.rotation = Quaternion.Slerp(target.rotation, targetRot, _visualLerpSpeed * Time.deltaTime);
        }

        private void ApplyFlickerEffect()
        {
            if (_model.IsInvulnerable)
            {
                var isWhiteFrame = Mathf.PingPong(Time.time * FlickerSpeed, 1f) > 0.5f;

                if (isWhiteFrame)
                {
                    SetSnakeColor(new Color(10f, 10f, 10f, 1f));
                }
                else
                {
                    SetSnakeColor(_originalColor);
                }
            }
            else
            {
                if (_headRenderer.color != _originalColor)
                {
                    SetSnakeColor(_originalColor);
                }
            }
        }

        private void SetSnakeColor(Color color) => _headRenderer.color = color;

    }
}