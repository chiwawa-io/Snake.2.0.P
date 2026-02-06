using System;
using System.Collections;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using TMPro;

namespace Game.UI
{
    public class GameView : BaseView
    {
        [Header("Gameplay Data")]
        [SerializeField] private TextMeshProUGUI scoreText;
        
        [Header("Lives System")]
        [SerializeField] private List<GameObject> lifeIcons;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI floatingScoreText;
        [SerializeField] private GameObject achievementNotificationRoot;
        [SerializeField] private TextMeshProUGUI achievementNameText;

        private void OnEnable()
        {
            GameManager.OnScoreChanged += UpdateScore;
            
            AchievGameListener.OnAchievementCompleted += ShowAchievementToast;
            
            Player.UpdateScore += ShowAddedScore;
            Player.UpdateLife += UpdateLives;
            Player.EventMessages += ShowFloatingScore;
        }

        private void OnDisable()
        {
            GameManager.OnScoreChanged -= UpdateScore;
            
            AchievGameListener.OnAchievementCompleted -= ShowAchievementToast;

            Player.UpdateScore -= ShowAddedScore;
            Player.UpdateLife -= UpdateLives;
            Player.EventMessages -= ShowFloatingScore;
        }

        public override void UpdateTimerDisplay(int secondsLeft) { }

        public void UpdateScore(int currentScore)
        {
            if (scoreText) scoreText.text = currentScore.ToString("D10");
        }

        public void UpdateLives(int currentLives)
        {
            currentLives = Mathf.Clamp(currentLives, 0, lifeIcons.Count);

            for (int i = 0; i < lifeIcons.Count; i++)
            {
                lifeIcons[i].SetActive(i < currentLives);
            }
        }

        private void ShowAddedScore(int amount, Vector2 position)
        {
            var message = $"+{amount}";
            ShowFloatingScore(message, position);
        }

        public void ShowFloatingScore(string message, Vector2 screenPosition)
        {
            if (floatingScoreText == null) return;

            floatingScoreText.transform.position = screenPosition;
            floatingScoreText.text = message;
            floatingScoreText.gameObject.SetActive(true);
            
            StopCoroutine(nameof(HideFloatingTextRoutine));
            StartCoroutine(nameof(HideFloatingTextRoutine));
        }

        public void ShowAchievementToast(string achievementName)
        {
            if (achievementNotificationRoot == null) return;

            achievementNameText.text = achievementName;
            achievementNotificationRoot.SetActive(true);
            
            CancelInvoke(nameof(HideAchievementToast));
            Invoke(nameof(HideAchievementToast), 3.0f);
        }

        private IEnumerator HideFloatingTextRoutine()
        {
            yield return new WaitForSeconds(0.8f);
            floatingScoreText.gameObject.SetActive(false);
        }

        private void HideAchievementToast()
        {
            achievementNotificationRoot.SetActive(false);
        }
    }
}