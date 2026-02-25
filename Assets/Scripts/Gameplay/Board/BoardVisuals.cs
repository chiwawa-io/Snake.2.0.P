using UnityEngine;

namespace Gameplay.Board
{
    public class BoardVisuals : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        //Temporary solution
        [SerializeField] private Sprite _easyBoard;
        [SerializeField] private Sprite _mediumBoard;
        [SerializeField] private Sprite _hardBoard;

        public void SetBoard(GameDifficulty gameDifficulty)
        {
            switch (gameDifficulty)
            {
                case GameDifficulty.Easy:
                    _spriteRenderer.sprite = _easyBoard;
                    break;
                case GameDifficulty.Medium:
                    _spriteRenderer.sprite = _mediumBoard;
                    break;
                case GameDifficulty.Hard:
                    _spriteRenderer.sprite = _hardBoard;
                    break;
            }
        }
    }
}