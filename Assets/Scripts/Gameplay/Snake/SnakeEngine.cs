using System.Linq;
using UnityEngine;

public class SnakeEngine
{
    private readonly SnakeModel _model;
    private readonly Vector2Int _gridSize;
    private readonly int _xBound;
    private readonly int _yBound;

    public SnakeEngine(SnakeModel model, Vector2 gridSize)
    {
        _model = model;
        _gridSize = Vector2Int.RoundToInt(gridSize);
        _xBound = _gridSize.x / 2;
        _yBound = _gridSize.y / 2;
    }

    public void Reset()
    {
        _model.Body.Clear();

        _model.Body.Add(new Vector2Int(0, -5));
        _model.Body.Add(new Vector2Int(0, -6));
        _model.Body.Add(new Vector2Int(0, -7));
        _model.Body.Add(new Vector2Int(0, -8));

        _model.Direction = Vector2Int.up;
        _model.PendingDirection = Vector2Int.up;
    }

    public void SetInput(Vector2Int dir)
    {
        if (dir + _model.Direction != Vector2Int.zero)
        {
            _model.PendingDirection = dir;
        }
    }

    public bool TickMovement(out Vector2Int newHeadPos)
    {
        _model.Direction = _model.PendingDirection;
        Vector2Int currentHead = _model.Body[0];
        newHeadPos = currentHead + _model.Direction;

        if (!_model.IsInvulnerable && !_model.IsRespawning)
        {
            if (Mathf.Abs(newHeadPos.x) > _xBound || Mathf.Abs(newHeadPos.y) > _yBound)
                return false;

            for (int i = 0; i < _model.Body.Count - 1; i++)
            {
                if (newHeadPos == _model.Body[i]) return false;
            }
        }

        _model.Body.Insert(0, newHeadPos);
        return true;
    }

    public void RemoveTail()
    {
        if (_model.Body.Count > 0)
        {
            _model.LastTailPosition = _model.Body.Last(); 
            _model.Body.RemoveAt(_model.Body.Count - 1);
        }
    }
}