using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameView : BaseView
{
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

        floatingText.transform.position = screenPosition;
        floatingText.text = message;
        floatingText.gameObject.SetActive(true);

        StopCoroutine(nameof(HideFloatingTextRoutine));
        StartCoroutine(nameof(HideFloatingTextRoutine));
    }

    private IEnumerator HideFloatingTextRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        floatingText.gameObject.SetActive(false);
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
        Invoke(nameof(HideAchievementToast), 3.0f);
    }

    private void HideAchievementToast()
    {
        achievementToastRoot.SetActive(false);
    }
}