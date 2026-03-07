using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UI.Global;
using UnityEngine;

namespace UI.Game.Views
{
    public class GameView : BaseView
    {
        private const float FloatDuration = 1.2f;
        private const float FloatDistance = 5f;
        private const float DurationMultiplicator = 0.5f;
        private const float PopupShowDuration = 2.5f;

        [Header("Gameplay Data")] 
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI lengthText;
        [SerializeField] private TextMeshProUGUI growthText;
        
        [Header("Visual")]
        [SerializeField] private GameObject scoreParent;
        [SerializeField] private GameObject lengthParent;
        [SerializeField] private GameObject growthParent;

        [Header("Lives System")] 
        [SerializeField] private List<GameObject> lifeIcons;

        [Header("Feedback")] 
        [SerializeField] private TextMeshProUGUI floatingText;
        [SerializeField] private GameObject achievementToastRoot;
        [SerializeField] private TextMeshProUGUI achievementNameText;

        public void SetScoreVisibility(bool isVisible) => scoreParent.gameObject.SetActive(isVisible);
        public void SetGrowthTimerVisibility(bool isVisible) => growthParent.gameObject.SetActive(isVisible);
        public void SetLengthVisibility(bool isVisible) => lengthParent.gameObject.SetActive(isVisible);
        public void SetScoreDisplay(int score) => scoreText.text = score.ToString("D10");
        public void SetLength(int length, int target) => lengthText.text = $"{length}/{target}";
        public void SetGrowthTimer(int timeRemaining) => growthText.text = timeRemaining.ToString();
        public void HideFloatingTextImmediate() => floatingText.gameObject.SetActive(false);

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
            mySequence.Join(floatingText.transform.DOMoveY(startPos.y + FloatDistance, FloatDuration)
                .SetEase(Ease.OutQuad));
            mySequence.Join(floatingText.DOFade(0f, FloatDuration * DurationMultiplicator)
                .SetDelay(FloatDuration * DurationMultiplicator));

            mySequence.OnComplete(() => { floatingText.gameObject.SetActive(false); });
        }

        public void ShowAchievementToast(string achievementName)
        {
            if (achievementToastRoot == null) return;

            achievementNameText.text = achievementName;
            achievementToastRoot.SetActive(true);

            CancelInvoke(nameof(HideAchievementToast));
            Invoke(nameof(HideAchievementToast), PopupShowDuration);
        }

        private void HideAchievementToast() => achievementToastRoot.SetActive(false);

    }
}