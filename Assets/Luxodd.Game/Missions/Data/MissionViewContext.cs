using Game.UI.Items.Missions;
using Luxodd.Game.Scripts.Missions;
using UnityEngine;

namespace Game.UI.Contexts
{
    public class MissionViewContext : MonoBehaviour
    {
        [field: SerializeField] public MissionDataBase MissionDataBase { get; private set; }
        [field: SerializeField] public MissionPreviewItemView MissionPreviewItemViewPrefab { get; private set; }
        [field: SerializeField] public MissionResultItemView MissionResultItemViewPrefab { get; private set; }
        
        [field: SerializeField] public Color PreviewColorDarkBlue { get; private set; }
        [field: SerializeField] public Color PreviewColorBlue { get; private set; }
    }
}
