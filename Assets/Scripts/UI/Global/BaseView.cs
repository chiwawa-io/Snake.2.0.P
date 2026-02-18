using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseView : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected GameObject _screenRoot;
    [SerializeField] protected Button _defaultFocusButton;
    [SerializeField] protected TextMeshProUGUI _timerText; 

    public virtual void Show()
    {
        if (_screenRoot) _screenRoot.SetActive(true);
        
        if (_defaultFocusButton != null)
        {
            _defaultFocusButton.Select();
            _defaultFocusButton.OnSelect(null);
        }
    }

    public virtual void Hide()
    {
        if (_screenRoot) _screenRoot.SetActive(false);
    }

    public virtual void UpdateTimer(string text)
    {
        if (_timerText) _timerText.text = text;
    }
}