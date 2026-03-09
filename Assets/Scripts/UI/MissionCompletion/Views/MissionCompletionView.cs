using System.Collections.Generic;
using TMPro;
using UI.Global;
using UI.MissionCompletion.Data;
using UnityEngine;

namespace UI.MissionCompletion.Views
{
    public class MissionCompletionView : BaseView
    {
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _container;
        [SerializeField] private MissionCompletionRowUI _rowPrefab;

        public void DisplayWonTitle()
        {
            _statusText.text = "You won!";
        }
        public void DisplayMissions(List<MissionCompletionUIData> missions)
        {
            foreach (Transform child in _container)
            {
                Destroy(child.gameObject);
            }

            foreach (var mission in missions)
            {
                var row = Instantiate(_rowPrefab, _container);
                row.Setup(mission.MissionName, mission.Description, mission.IsCompleted);
            }
        }
    }
}