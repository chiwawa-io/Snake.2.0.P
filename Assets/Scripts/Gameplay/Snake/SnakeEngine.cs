using System.Linq;
using UnityEngine;

namespace Gameplay.Snake
{
    public class SnakeEngine
    {
        private readonly Vector2Int InitHeadPos = new(0, -5);
        private readonly Vector2Int InitNeckPos = new(0, -6);
        private readonly Vector2Int InitBodyPos = new(0, -7);
        private readonly Vector2Int InitTailPos = new(0, -8);

        private readonly SnakeModel _model;
        private Vector2Int _gridSize;
        private int _xBound;
        private int _yBound;

        public SnakeEngine(SnakeModel model)
        {
            _model = model;
        }

        public void Initialize(Vector2 gridSize)
        {
            _gridSize = Vector2Int.RoundToInt(gridSize);
            _xBound = _gridSize.x / 2;
            _yBound = _gridSize.y / 2;
        }

        public void Reset()
        {
            _model.Body.Clear();

            _model.Body.Add(InitHeadPos);
            _model.Body.Add(InitNeckPos);
            _model.Body.Add(InitBodyPos);
            _model.Body.Add(InitTailPos);

            _model.Direction = Vector2Int.up;
            _model.PendingDirection = Vector2Int.up;

            _model.LastTailPosition = _model.Body.Last();
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

            if (Mathf.Abs(newHeadPos.x) > _xBound || Mathf.Abs(newHeadPos.y) > _yBound)
                return false;

            if (!_model.IsRespawning)
            {
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
}