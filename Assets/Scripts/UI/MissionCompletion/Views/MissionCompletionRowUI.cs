using TMPro;
using UnityEngine;

namespace UI.MissionCompletion.Views
{
    public class MissionCompletionRowUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Color _winColor = Color.green;
        [SerializeField] private Color _lossColor = Color.red;

        public void Setup(string missionName, string description, bool isCompleted)
        {
            if (_nameText) _nameText.text = missionName;
            if (_descriptionText) _descriptionText.text = description;
            
            if (_statusText)
            {
                _statusText.text = isCompleted ? "COMPLETED" : "FAILED";
                _statusText.color = isCompleted ? _winColor : _lossColor;
            }
        }
    }
}