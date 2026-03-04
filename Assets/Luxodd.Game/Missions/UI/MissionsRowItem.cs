using Luxodd.Game.HelpersAndUtils.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Scripts.Missions
{
    public class MissionsRowItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Transform _completedState;
        [SerializeField] private Transform _failedState;
        [SerializeField] private Transform _progressParent;
        [SerializeField] private Toggle _missionSuccessfullyComplete;
        [SerializeField] private Image _greenBackground;
        [SerializeField] private Image _redBackground;

        [Header("Animation Settings")]
        [SerializeField] private float _animDuration = 0.4f;

        private bool _successfullyComplete;
        private int _maxValue;

        public bool SuccessfullyComplete => _successfullyComplete;

        private void ResetBackgrounds()
        {
            _greenBackground.gameObject.SetActive(false);
            _redBackground.gameObject.SetActive(false);
            _greenBackground.fillAmount = 0;
            _redBackground.fillAmount = 0;
        }

        public void SetDescription(string description)
        {
            _descriptionText.text = string.Format(description, _maxValue);
        }

        public void SetMissionState(IReadOnlyProperty<MissionState> state)
        {
            state.AddListener(OnMissionStateChanged);
        }

        public void SetMissionProgressMaxValue(int value)
        {
            _maxValue = value;
        }

        public void SetResult(MissionState state, string description = null)
        {
            if (!string.IsNullOrEmpty(description))
                SetDescription(description);

            HandleStateAnimation(state);
        }

        public void SetProgress(int progressValue)
        {
            _progressText.text = $"{progressValue}/{_maxValue}";
            _progressParent.gameObject.SetActive(true);
        }

        private void OnMissionStateChanged(MissionState state)
        {
            HandleStateAnimation(state);
        }


        private void HandleStateAnimation(MissionState state)
        {
            ResetBackgrounds();

            switch (state)
            {
                case MissionState.Completed:
                    AnimateState(_completedState);
                    _failedState.gameObject.SetActive(false);
                    AnimateBackground(_greenBackground);
                    break;

                case MissionState.Failed:
                    AnimateState(_failedState);
                    _completedState.gameObject.SetActive(false);
                    AnimateBackground(_redBackground);
                    break;

                default:
                    _completedState.gameObject.SetActive(false);
                    _failedState.gameObject.SetActive(false);
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
