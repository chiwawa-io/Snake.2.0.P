using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Items.Missions
{
    public class MissionPreviewItemView : MonoBehaviour
    {
        private const string BetFormat = "${0:F2}";
        private const string RatioFormat = "x{0:F2}";
        
        [SerializeField] private TMP_Text _missionNameText;
        [SerializeField] private TMP_Text _missionBetText;
        [SerializeField] private TMP_Text _missionRatioText;
        [SerializeField] private TMP_Text _missionPayoutText;
        
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Toggle _completedToggle;

        private int _missionId;
        private Action _onToggleChanged;

        private void Start()
        {
            _completedToggle.onValueChanged.AddListener(OnToggleChanged);
        }
        public void SetMissionData(string missionName, float bet, float ratio, float payout)
        {
            _missionNameText.text = missionName;
            _missionBetText.text = string.Format(BetFormat, bet);
            _missionRatioText.text = string.Format(RatioFormat, ratio);
            _missionPayoutText.text = string.Format(BetFormat, payout);
        }

        public void SetToggleCallBack(Action action)
        {
            _onToggleChanged = action;
        }

        private void OnToggleChanged(bool value)
        {
            _onToggleChanged?.Invoke();
        }

        public void SetBackgroundColor(Color color)
        {
            _backgroundImage.color = color;
        }
    }
}
