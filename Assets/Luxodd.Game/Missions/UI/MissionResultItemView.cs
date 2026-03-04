using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Items.Missions
{
    public class MissionResultItemView : MonoBehaviour
    {
        private const string BetFormat = "${0:F2}";
        private const string RatioFormat = "x{0:F2}";
        
        [SerializeField] private TMP_Text _missionNameText;
        [SerializeField] private TMP_Text _missionBetText;
        [SerializeField] private TMP_Text _missionRatioText;
        [SerializeField] private TMP_Text _missionPayoutText;
        [SerializeField] private Transform _statusCompleted;
        [SerializeField] private Transform _statusFailed;
        
        [SerializeField] private Image _backgroundImage;

        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _failedColor = Color.red;



        public void SetMissionData(string missionName, float bet, float ratio, float payout, bool status)
        {
            _missionNameText.text = missionName;
            _missionBetText.text = string.Format(BetFormat, bet);
            //_missionRatioText.text = string.Format(RatioFormat, ratio);
            _missionPayoutText.text = string.Format(BetFormat, payout);

            _statusCompleted.gameObject.SetActive(status);
            _statusFailed.gameObject.SetActive(!status);

            _missionPayoutText.color = status ? _completedColor : _failedColor;
        }

        public void SetBackgroundColor(Color color)
        {
            _backgroundImage.color = color;
        }
    }
}
