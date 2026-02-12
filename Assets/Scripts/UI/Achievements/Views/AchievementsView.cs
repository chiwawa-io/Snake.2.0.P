using System;
using System.Collections.Generic;
using TMPro;
using UI.Achievements.Logic;
using UI.Achievements.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Achievements.Views
{
    public class AchievementsView : BaseView
    {
        [Header("References")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI pageText;
        
        [Header("Grid")]
        [SerializeField] private Transform gridRoot;
        [SerializeField] private GameObject itemPrefab; // The row prefab

        // Events for the Presenter to listen to
        public event Action OnNextClicked;
        public event Action OnPrevClicked;
        public event Action OnCloseClicked;

        [Inject] private AchievementsPresenter _presenter;

        private void Start()
        {
            nextButton.onClick.AddListener(() => OnNextClicked?.Invoke());
            prevButton.onClick.AddListener(() => OnPrevClicked?.Invoke());
            closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
            _presenter.Initialize(); 
        }

        public override void Show()
        {
            base.Show();
            _presenter.Show(); 
        }

        public void UpdateNavigation(bool canGoBack, bool canGoForward, int current, int total)
        {
            prevButton.interactable = canGoBack;
            nextButton.interactable = canGoForward;
            if(pageText) pageText.text = $"{current} / {total}";
        }

        public void RenderItems(List<AchievementDisplayData> items)
        {
            foreach (Transform child in gridRoot) Destroy(child.gameObject);

            foreach (var item in items)
            {
                var go = Instantiate(itemPrefab, gridRoot);
                var script = go.GetComponent<AchievementsRowUI>();
                script.Setup(item.Name, item.IsUnlocked, item.Description); 
            }
        }
    }
}

