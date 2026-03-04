using Game.UI.Items.Missions;
using Luxodd.Game.Scripts.Missions.Testing;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Luxodd.Game.Scripts.Missions.Testing
{
    public class MissionResultWindowHandler : MonoBehaviour
    {
        [SerializeField] private Button _nextButton;

        private Action _onNextButtonClickedCallback;
        public List<MissionResultItemView> Items { get; private set; } = new();

        public void ClearMissionPreviewItems() => Items.Clear();

        public void AddMissionPreviewItem(MissionResultItemView item) => Items.Add(item);
        public void SetNextButtonClickedCallback(Action onNextButtonClickedCallback)
        {
            _onNextButtonClickedCallback = onNextButtonClickedCallback;
        }
        public void ShowPanel()
        {
            gameObject.SetActive(true);
        }

        public void HidePanel()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            _nextButton.onClick.AddListener(OnNextButtonClickedHandler);
        }

        private void OnNextButtonClickedHandler()
        {
            _onNextButtonClickedCallback.Invoke();
        }
    }
}