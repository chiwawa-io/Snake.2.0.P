using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Luxodd.Game.HelpersAndUtils.Utils;

namespace Luxodd.Game.Scripts.Missions
{
    public class MissionsProgressItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Transform _completedState;
        [SerializeField] private Transform _failedState;
        [SerializeField] private Transform _progressParent;
        [SerializeField] private Toggle _missionSuccessfullyComplete;
        [SerializeField] private Image _sliderProgress;
        [SerializeField] private Transform _completedStatus;
        [SerializeField] private Image _greenBackground;

        [Header("Animation Settings")]
        [SerializeField] private float _animDuration = 0.4f;

        private bool _successfullyComplete;
        private float _maxValue;

        public bool SuccessfullyComplete => _successfullyComplete;


        public void SetDescription(string description)
        {
            _descriptionText.text = string.Format(description, _maxValue);
        }

        public void SetMissionState(IReadOnlyProperty<MissionState> state)
        {
            state.AddListener(OnMissionStateChanged);
        }

        public void SetMissionProgressValue(IIntReadOnlyProperty progressValue)
        {

        }

        public void SetMissionProgressMaxValue(float value)
        {
            _maxValue = value;
        }

        public void SetResult(MissionState state, string description = null)
        {
            if (!string.IsNullOrEmpty(description))
                SetDescription(description);

            HandleStateAnimation(state);
        }

        public void SetProgressValue(int progressValue)
        {
            _progressText.text = $"{progressValue}"; 
            _sliderProgress.fillAmount = Mathf.Clamp01(progressValue / _maxValue);
            _progressParent.gameObject.SetActive(true);
        }

        private void OnMissionStateChanged(MissionState state)
        {
            HandleStateAnimation(state);
        }

        private void HandleStateAnimation(MissionState state)
        {

            switch (state)
            {
                case MissionState.Completed:
                    AnimateState(_completedState);
                    _failedState.gameObject.SetActive(false);
                    AnimateBackground(_greenBackground);
                    break;
            }
        }

        private void AnimateState(Transform target)
        {
        }

        private void AnimateBackground(Image bg)
        {
        }

        private void Awake()
        {
            if (_missionSuccessfullyComplete != null)
            {
                _missionSuccessfullyComplete.onValueChanged.AddListener(isOn =>
                {
                    _successfullyComplete = isOn;
                    SetResult(isOn ? MissionState.Completed : MissionState.Failed);
                });
            }
        }

    }
}
