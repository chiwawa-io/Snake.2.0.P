using System.Collections;
using Core.Enums;
using Core.Events;
using UnityEngine;
using Zenject;

namespace Utilities
{
    public class InactivityDetector : MonoBehaviour
    {
        [SerializeField] private int inactivityTimeLimit = 30;

        private float _currentTime;
        private bool _isTimerRunning; 
        private Coroutine _timerCoroutine;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Start()
        {
            _signalBus?.Subscribe<GameStateChangedSignal>(StartDetector);
        }

        public void OnDisable()
        {
            _signalBus?.Unsubscribe<GameStateChangedSignal>(StartDetector);
        }

        private void StartDetector(GameStateChangedSignal signal)
        {
            _currentTime = 0f;
            
            if (signal.NewState == GameState.InGame)
            {
                StopDetector();   
                return;
            }

            _isTimerRunning = true;
            
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }
            _timerCoroutine = StartCoroutine(UpdateTimerDisplay());
            
            Debug.LogWarning("Inactivity Detector Started.");
        }

        private void StopDetector()
        {
            _isTimerRunning = false;
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }

            Debug.LogWarning("Inactivity Detector Stopped.");
        }

        void Update()
        {
            if (!_isTimerRunning)
            {
                return;
            }

            _currentTime += Time.deltaTime;

            if (_currentTime >= inactivityTimeLimit)
            {
                Debug.LogWarning("Inactivity time limit reached!.");
                _signalBus.Fire(new InactivityTimeOut());
                StopDetector(); 
            }
        }
        IEnumerator UpdateTimerDisplay()
        {
            while (_isTimerRunning)
            {
                var timeRemaining = Mathf.RoundToInt(inactivityTimeLimit - _currentTime);
                
                if (timeRemaining < 0) timeRemaining = 0;

                Debug.LogWarning(timeRemaining);
                _signalBus.Fire(new InactivityTimerSignal { SecondsLeft = timeRemaining });
                
                yield return new WaitForSeconds(1f);
            }
        }
    }
}

