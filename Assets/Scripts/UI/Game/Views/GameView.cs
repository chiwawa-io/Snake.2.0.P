using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameView : BaseView
{
    private const float FloatDuration = 1.2f;
    private const float FloatDistance = 5f;
    private const float DurationMultiplicator = 0.5f;
    private const float PopupShowDuration = 3f;
    
    [Header("Gameplay Data")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Lives System")]
    [SerializeField] private List<GameObject> lifeIcons;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI floatingText;
    [SerializeField] private GameObject achievementToastRoot;
    [SerializeField] private TextMeshProUGUI achievementNameText;

    public void SetScoreDisplay(int score)
    {
        if (scoreText) scoreText.text = score.ToString("D10");
    }

    public void SetLives(int currentLives)
    {
        int safeCount = Mathf.Clamp(currentLives, 0, lifeIcons.Count);

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            lifeIcons[i].SetActive(i < safeCount);
        }
    }

    public void ShowFloatingText(string message, Vector2 screenPosition)
    {
        if (floatingText == null) return;

        floatingText.DOKill(); 
        floatingText.transform.DOKill();
        
        Vector3 startPos = new Vector3(screenPosition.x, screenPosition.y, floatingText.transform.position.z);
        floatingText.transform.position = startPos;
        floatingText.alpha = 1f; 
        floatingText.text = message;
        
        floatingText.gameObject.SetActive(true);

        Sequence mySequence = DOTween.Sequence();
        mySequence.Join(floatingText.transform.DOMoveY(startPos.y + FloatDistance, FloatDuration).SetEase(Ease.OutQuad));
        mySequence.Join(floatingText.DOFade(0f, FloatDuration * DurationMultiplicator).SetDelay(FloatDuration * DurationMultiplicator));

        mySequence.OnComplete(() => 
        {
            floatingText.gameObject.SetActive(false);
        });
    }

    public void HideFloatingTextImmediate()
    {
        if(floatingText) floatingText.gameObject.SetActive(false);
    }

    public void ShowAchievementToast(string achievementName)
    {
        if (achievementToastRoot == null) return;

        achievementNameText.text = achievementName;
        achievementToastRoot.SetActive(true);

        CancelInvoke(nameof(HideAchievementToast));
        Invoke(nameof(HideAchievementToast), PopupShowDuration);
    }

    private void HideAchievementToast()
    {
        achievementToastRoot.SetActive(false);
    }
}