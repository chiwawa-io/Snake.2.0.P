using System;
using Luxodd.Game.Scripts.Missions;
using TMPro;
using UnityEngine;

namespace Game.UI.Items.Missions
{
    public class MissionTypeItemView : MonoBehaviour
    {
        private const string PrimaryMission = "Primary Mission";
        private const string DependentMission = "Dependent Mission";
        private const string IndependentMission = "Independent Mission";
        
        [SerializeField] private TMP_Text _itemNameText;

        public void SetMissionType(MissionType missionType)
        {
            _itemNameText.text = GetMissionTypeText(missionType);
        }

        private string GetMissionTypeText(MissionType missionType)
        {
            return missionType switch
            {
                MissionType.Primary => PrimaryMission,
                MissionType.Independent => IndependentMission,
                MissionType.Dependent => DependentMission,
                _ => throw new ArgumentOutOfRangeException(nameof(missionType), missionType, null)
            };
        }
    }
}
