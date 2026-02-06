using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ErrorView : BaseView
    {
        [SerializeField] private TextMeshProUGUI codeText;
        [SerializeField] private TextMeshProUGUI messageText;

        public void SetErrorDetails(string code, string message)
        {
            codeText.text = $"Error Code: {code}";
            messageText.text = message;
        }
    }
}