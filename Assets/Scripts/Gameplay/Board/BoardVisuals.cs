using System.Collections.Generic;
using Core.Enums;
using Gameplay.Global.Data;
using UnityEngine;

namespace Gameplay.Board
{
    public class BoardVisuals : MonoBehaviour
    {[Header("Settings")]
        [SerializeField] private GameObject _tilePrefab; 
        [SerializeField] private Transform _boardContainer; 

        [Header("Styles")]
        [SerializeField] private BoardStyle _easyStyle;
        [SerializeField] private BoardStyle _mediumStyle;
        [SerializeField] private BoardStyle _hardStyle;

        private readonly List<GameObject> _spawnedTiles = new();

        public void GenerateBoard(Vector2Int bounds, GameDifficulty difficulty)
        {
            ClearBoard();

            BoardStyle style = GetStyleForDifficulty(difficulty);
            if (style == null)
            {
                Debug.LogError("BoardStyle is missing!");
                return;
            }

            int xBound = bounds.x / 2;
            int yBound = bounds.y / 2;
            
            int visualXBound = xBound + 1;
            int visualYBound = yBound + 1;

            for (int x = -visualXBound; x <= visualXBound; x++)
            {
                for (int y = -visualYBound; y <= visualYBound; y++)
                {
                    Sprite spriteToUse = DetermineTileSprite(x, y, visualXBound, visualYBound, style);
                    SpawnTile(new Vector2(x, y), spriteToUse);
                }
            }
        }

        private void ClearBoard()
        {
            foreach (var tile in _spawnedTiles)
            {
                if (tile != null) Destroy(tile);
            }
            _spawnedTiles.Clear();
        }

        private BoardStyle GetStyleForDifficulty(GameDifficulty difficulty)
        {
            return difficulty switch
            {
                GameDifficulty.Easy => _easyStyle,
                GameDifficulty.Medium => _mediumStyle,
                GameDifficulty.Hard => _hardStyle,
                _ => _easyStyle
            };
        }

        private Sprite DetermineTileSprite(int x, int y, int maxX, int maxY, BoardStyle style)
        {
            bool isTop = (y == maxY);
            bool isBottom = (y == -maxY);
            bool isRight = (x == maxX);
            bool isLeft = (x == -maxX);

            // Corners
            if (isTop && isLeft) return style.TopLeftCorner;
            if (isTop && isRight) return style.TopRightCorner;
            if (isBottom && isLeft) return style.BottomLeftCorner;
            if (isBottom && isRight) return style.BottomRightCorner;

            // Borders
            if (isTop) return style.TopBorder;
            if (isBottom) return style.BottomBorder;
            if (isLeft) return style.LeftBorder;
            if (isRight) return style.RightBorder;

            // Center
            return style.CenterTile;
        }

        private void SpawnTile(Vector2 position, Sprite sprite)
        {
            if (sprite == null) return;

            GameObject tile = Instantiate(_tilePrefab, position, Quaternion.identity, _boardContainer);
            tile.name = $"Tile_{position.x}_{position.y}";
            
            if (tile.TryGetComponent(out SpriteRenderer sr))
            {
                sr.sprite = sprite;
            }

            _spawnedTiles.Add(tile);
        }
    }
}