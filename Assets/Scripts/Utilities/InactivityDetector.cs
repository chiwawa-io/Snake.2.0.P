using System.Collections;
using Core.Enums;
using Core.Events;
using UnityEngine;
using Zenject;

namespace Utilities
{
    public class InactivityDetector : MonoBehaviour
    {
        [SerializeField] private int _inactivityTimeLimit = 30;

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
            StartDetector();
            _signalBus?.Subscribe<GameStateChangedSignal>(UpdateGameState);
        }

        private void Update()
        {
            if (!_isTimerRunning) return;

            _currentTime += Time.deltaTime;

            if (_currentTime >= _inactivityTimeLimit)
            {
                Debug.LogWarning("Inactivity time limit reached!.");

                _signalBus.Fire(new GameStateChangedSignal(GameState.InGame));
                
                StopDetector(); 
            }
        }

        private void OnDisable()
        {
            _signalBus?.Unsubscribe<GameStateChangedSignal>(UpdateGameState);
        }

        private void StartDetector()
        {
            _currentTime = 0f;

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

        private IEnumerator UpdateTimerDisplay()
        {
            while (_isTimerRunning)
            {
                var timeRemaining = Mathf.RoundToInt(_inactivityTimeLimit - _currentTime);
                if (timeRemaining < 0) timeRemaining = 0;

                Debug.LogWarning(timeRemaining);
                _signalBus.Fire(new InactivityTimerSignal (timeRemaining));
                
                yield return new WaitForSeconds(1f);
            }
        }
        private void UpdateGameState(GameStateChangedSignal signal)
        {
            if (signal.NewState == GameState.InGame)
            {
                StopDetector();
            }
        }
    }
}