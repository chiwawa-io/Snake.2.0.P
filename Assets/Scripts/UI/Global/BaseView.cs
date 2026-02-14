using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseView : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected GameObject screenRoot;
    [SerializeField] protected Button defaultFocusButton;
    [SerializeField] protected TextMeshProUGUI timerText; 

    public virtual void Show()
    {
        if (screenRoot) screenRoot.SetActive(true);
        
        if (defaultFocusButton != null)
        {
            defaultFocusButton.Select();
            defaultFocusButton.OnSelect(null);
        }
    }

    public virtual void Hide()
    {
        if (screenRoot) screenRoot.SetActive(false);
    }

    public virtual void UpdateTimer(string text)
    {
        if (timerText) timerText.text = text;
    }
}