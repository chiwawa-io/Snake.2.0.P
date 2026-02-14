using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class SnakeView : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject snakeHeadVisual;
    [SerializeField] private GameObject snakeTailVisual;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject boomEffect; 
    [SerializeField] private float visualLerpSpeed = 8f;

    private SnakeModel _model;
    private Vector2 _lastTailPosition;
    
    [Inject]
    public void Construct(SnakeModel model)
    {
        _model = model;
    }

    public void ToggleVisuals(bool isActive)
    {
        snakeHeadVisual.SetActive(isActive);
        snakeTailVisual.SetActive(isActive);
        lineRenderer.gameObject.SetActive(isActive);
    }

    public void PlayBoomEffect()
    {
        if (boomEffect)
        {
            boomEffect.transform.position = snakeHeadVisual.transform.position;
            boomEffect.SetActive(true);
        }
    }

    public void UpdateVisuals(float interpolationFactor)
    {
        if (_model.Body.Count < 2) return;

        List<Vector3> visualPoints = new List<Vector3>();

        Vector2 visualHeadPos = Vector2.Lerp(_model.Body[1], _model.Body[0], interpolationFactor);
        snakeHeadVisual.transform.position = visualHeadPos;
        visualPoints.Add(visualHeadPos);
        UpdateRotation(snakeHeadVisual.transform, _model.Body[0], _model.Body[1]);

        for (int i = 1; i < _model.Body.Count - 1; i++)
        {
            Vector2 prevPos = _model.Body[i + 1];
            Vector2 currentPos = _model.Body[i];
            visualPoints.Add(Vector2.Lerp(prevPos, currentPos, interpolationFactor));
        }
        
        Vector2 tailCurrentPos = _model.Body.Last();
        Vector2 visualTailPos = Vector2.Lerp(_model.LastTailPosition, tailCurrentPos, interpolationFactor);
        snakeTailVisual.transform.position = visualTailPos;
        visualPoints.Add(visualTailPos);
        UpdateRotation(snakeTailVisual.transform, _model.Body[^2], _model.Body.Last());

        lineRenderer.positionCount = visualPoints.Count;
        lineRenderer.SetPositions(visualPoints.ToArray());
    }

    private void UpdateRotation(Transform target, Vector2 to, Vector2 from)
    {
        Vector2 dir = to - from;
        if (dir == Vector2.zero) return; 

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, angle - 90);
        target.rotation = Quaternion.Slerp(target.rotation, targetRot, visualLerpSpeed * Time.deltaTime);
    }
}