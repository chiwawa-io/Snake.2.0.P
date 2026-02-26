using UnityEngine;

namespace Gameplay.Global.Data
{[CreateAssetMenu(fileName = "New Board Style", menuName = "Snake/Board Style")]
    public class BoardStyle : ScriptableObject
    {
        [Header("Center")]
        [field: SerializeField] public Sprite CenterTile { get; private set; }

        [Header("Corners")]
        [field: SerializeField] public Sprite TopLeftCorner { get; private set; }
        [field: SerializeField] public Sprite TopRightCorner { get; private set; }
        [field: SerializeField] public Sprite BottomLeftCorner { get; private set; }[field: SerializeField] public Sprite BottomRightCorner { get; private set; }

        [Header("Borders")]
        [field: SerializeField] public Sprite TopBorder { get; private set; }[field: SerializeField] public Sprite BottomBorder { get; private set; }
        [field: SerializeField] public Sprite LeftBorder { get; private set; }
        [field: SerializeField] public Sprite RightBorder { get; private set; }
    }
}