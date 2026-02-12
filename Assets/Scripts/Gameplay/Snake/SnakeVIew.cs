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

    public void PlayBoomEffect(Vector2 position)
    {
        if (boomEffect)
        {
            boomEffect.transform.position = position;
            boomEffect.SetActive(true);
        }
    }

    public void UpdateVisuals(float lerpRatio)
    {
        if (_model == null || _model.Body.Count < 2) return;

        List<Vector3> visualPoints = new List<Vector3>();

        Vector2 visualHeadPos = Vector2.Lerp(_model.Body[1], _model.Body[0], lerpRatio);
        snakeHeadVisual.transform.position = visualHeadPos;
        visualPoints.Add(visualHeadPos);

        UpdateRotation(snakeHeadVisual.transform, _model.Body[0], _model.Body[1], 1f);

        for (int i = 1; i < _model.Body.Count - 1; i++)
        {
            Vector2 prevPos = _model.Body[i + 1];
            Vector2 currentPos = _model.Body[i];
            visualPoints.Add(Vector2.Lerp(prevPos, currentPos, lerpRatio));
        }

        Vector2 visualTailPos = _model.Body.Last(); 
        
        visualPoints.Add(visualTailPos);
        snakeTailVisual.transform.position = visualTailPos;

        if (_model.Body.Count > 1)
        {
            UpdateRotation(snakeTailVisual.transform, _model.Body[^2], _model.Body.Last(), 2f);
        }

        lineRenderer.positionCount = visualPoints.Count;
        lineRenderer.SetPositions(visualPoints.ToArray());
    }

    private void UpdateRotation(Transform target, Vector2 to, Vector2 from, float speedMult)
    {
        Vector2 dir = to - from;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, angle - 90);
        target.rotation = Quaternion.Slerp(target.rotation, targetRot, visualLerpSpeed * Time.deltaTime * speedMult);
    }
}