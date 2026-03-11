using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UI.Global;
using UnityEngine;
using UnityEngine.UI;

public class GameView : BaseView
{
    private const float ToastFadeDuration = 0.5f;
    private const float ToastDisplayTime = 2.0f;
    private const float VignettePulseSpeed = 15f;[Header("Gameplay Data")]
    [SerializeField] private TextMeshProUGUI _scoreText;[Header("Lives System")]
    [SerializeField] private List<GameObject> _lifeIcons;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI _floatingText;
    [SerializeField] private float _floatDuration = 1.2f;
    [SerializeField] private GameObject _achievementToastRoot;
    [SerializeField] private TextMeshProUGUI _achievementNameText;

    [Header("SB Mode UI")]
    [SerializeField] private TextMeshProUGUI _goalText;
    [SerializeField] private GameObject _growthParent;
    [SerializeField] private TextMeshProUGUI _growthText;
    
    [Header("SB Pressure Feedback")]
    [SerializeField] private Image _vignetteImage; 

    [Header("Mission Briefing")]
    [SerializeField] private CanvasGroup _briefingGroup; 
    [SerializeField] private TextMeshProUGUI _briefingTitleText;
    [SerializeField] private TextMeshProUGUI _briefingRulesText;

    public void SetScoreDisplay(int score)
    {
        if (_scoreText) _scoreText.text = score.ToString("D10");
    }

    public void SetLives(int currentLives)
    {
        int safeCount = Mathf.Clamp(currentLives, 0, _lifeIcons.Count);

        for (int i = 0; i < _lifeIcons.Count; i++)
        {
            _lifeIcons[i].SetActive(i < safeCount);
        }
    }

    public void ShowFloatingText(string message, Vector2 screenPosition)
    {
        if (_floatingText == null) return;

        _floatingText.DOKill(); 
        _floatingText.transform.DOKill();
        
        Vector3 startPos = new Vector3(screenPosition.x, screenPosition.y, _floatingText.transform.position.z);
        _floatingText.transform.position = startPos;
        _floatingText.alpha = 1f; 
        _floatingText.text = message;
        
        _floatingText.gameObject.SetActive(true);

        Sequence mySequence = DOTween.Sequence();
        mySequence.Join(_floatingText.transform.DOMoveY(startPos.y + 5f, _floatDuration).SetEase(Ease.OutQuad));
        mySequence.Join(_floatingText.DOFade(0f, _floatDuration * 0.5f).SetDelay(_floatDuration * 0.5f));
        mySequence.OnComplete(() => 
        {
            _floatingText.gameObject.SetActive(false);
        });
    }

    public void HideFloatingTextImmediate()
    {
        if(_floatingText) _floatingText.gameObject.SetActive(false);
    }

    public void ShowAchievementToast(string achievementName)
    {
        if (_achievementToastRoot == null) return;

        _achievementToastRoot.transform.DOKill();
        _achievementNameText.text = achievementName;
        _achievementToastRoot.SetActive(true);

        _achievementToastRoot.transform.localScale = Vector3.zero;

        Sequence toastSequence = DOTween.Sequence();
        toastSequence.Append(_achievementToastRoot.transform.DOScale(Vector3.one, ToastFadeDuration).SetEase(Ease.OutBack));
        toastSequence.AppendInterval(ToastDisplayTime);
        toastSequence.Append(_achievementToastRoot.transform.DOScale(Vector3.zero, ToastFadeDuration).SetEase(Ease.InBack));
        toastSequence.OnComplete(() => _achievementToastRoot.SetActive(false));
    }

    public void ToggleSbUI()
    {
        if (_vignetteImage != null) _vignetteImage.gameObject.SetActive(false);
        if (_growthText != null) _growthParent.SetActive(true);
    }

    public void UpdateGoal(int current, int target)
    {
        if (_goalText == null) return;
        _goalText.text = $"{current} / {target}";
        _goalText.transform.DOKill();
        _goalText.transform.DOScale(1.2f, 0.1f).OnComplete(() => _goalText.transform.DOScale(1f, 0.1f));
    }

    public void UpdateTimer(int timeRemaining, bool isCritical)
    {
        _growthText.text = timeRemaining.ToString();

        if (_vignetteImage != null)
        {
            if (isCritical)
            {
                if (!_vignetteImage.gameObject.activeSelf) _vignetteImage.gameObject.SetActive(true);
                
                float alpha = (Mathf.Sin(Time.time * VignettePulseSpeed) + 1f) / 2f * 0.4f; 
                var c = _vignetteImage.color;
                c.a = alpha;
                _vignetteImage.color = c;
            }
            else
            {
                if (_vignetteImage.gameObject.activeSelf) _vignetteImage.gameObject.SetActive(false);
            }
        }
    }

    public void ShowBriefing(int targetLength)
    {
        if (_briefingGroup == null) return;
        
        _briefingTitleText.text = $"PRIMARY MISSION:\nREACH LENGTH {targetLength}";
        _briefingRulesText.text = $"RULES:\n- 1 Life Only\n- Timer Expiry = Speed Penalty\n- Don't Crash!";
        
        _briefingGroup.gameObject.SetActive(true);
        _briefingGroup.alpha = 0f;
        
        Sequence seq = DOTween.Sequence();
        seq.Append(_briefingGroup.DOFade(1f, 0.5f));
        seq.AppendInterval(2.5f); 
        seq.Append(_briefingGroup.DOFade(0f, 0.5f));
        seq.OnComplete(() => _briefingGroup.gameObject.SetActive(false));
    }

    private void HideAchievementToast()
    {
        _achievementToastRoot.SetActive(false);
    }
}