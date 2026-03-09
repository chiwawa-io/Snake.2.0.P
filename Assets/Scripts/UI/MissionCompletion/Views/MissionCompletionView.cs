using System.Collections.Generic;
using UI.Global;
using UI.MissionCompletion.Data;
using UI.MissionCompletion.Views;
using UnityEngine;

namespace UI.MissionCompletion.Views
{
    public class MissionCompletionView : BaseView
    {
        [SerializeField] private Transform _container;
        [SerializeField] private MissionCompletionRowUI _rowPrefab;

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