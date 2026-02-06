using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    public abstract class BaseView : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] private GameObject screenRoot;
        [SerializeField] private Button defaultFocusButton;
        [SerializeField] protected TextMeshProUGUI timerText;

        public bool IsVisible => screenRoot != null && screenRoot.activeSelf;

        public virtual void Show()
        {
            if (screenRoot) screenRoot.SetActive(true);
            InactivityDetector.OnTimerUpdate += UpdateTimerDisplay;
            if (defaultFocusButton != null) 
            {
                defaultFocusButton.Select();
                defaultFocusButton.OnSelect(null); 
            }

            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            InactivityDetector.OnTimerUpdate -= UpdateTimerDisplay;
            if (screenRoot) screenRoot.SetActive(false);
        }
        public virtual void UpdateTimerDisplay(int secondsLeft)
        {
            if (timerText != null)
            {
                timerText.text = secondsLeft.ToString();
            }
        }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}