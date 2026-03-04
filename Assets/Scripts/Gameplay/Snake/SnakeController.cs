using System;
using Core.Enums;
using Core.Events;
using Gameplay.GameItems;
using SBetting;
using Services.Audio;
using UnityEngine;
using Zenject;

namespace Gameplay.Snake
{
    public class SnakeController : IInitializable, ITickable, IDisposable
    {
        private const float BaseFrequency = 0.1f;
        private const float SpeedUpFrequency = 0.08f;
        private const float RespawnDelay = 0.4f;
        private const float InvulnerableAfterRespawn = 1.0f;
        private const int StartingLives = 3;
        private const float FloatTolerance = 0.001f;

        private readonly SignalBus _signalBus;
        private readonly SnakeEngine _engine;
        private readonly SnakeModel _model;
        private readonly ItemSpawner _itemSpawner;
        private readonly SnakeView _view;
        private readonly SbPressureManager _sbPressure;

        private float _moveTimer;
        private float _powerUpTimer;
        private float _respawnTimer;
        private float _invulnerableTimer;
        private bool _gameIsRunning;
        private int _lives;
        private int _score;
        private int _targetLength;
        private bool _isSbMode;
        public float InterpolationFactor => _gameIsRunning ? (_moveTimer / _model.MoveFrequency) : 0f;

        public SnakeController(
            SignalBus signalBus,
            SnakeEngine engine,
            SnakeModel model,
            ItemSpawner spawner,
            SnakeView view,
            SbPressureManager sbPressure)
        {
            _signalBus = signalBus;
            _engine = engine;
            _model = model;
            _itemSpawner = spawner;
            _view = view;
            _sbPressure = sbPressure;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<InputDirectionSignal>(OnInput);
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<RevivePlayerSignal>(OnRevive);
            _signalBus.Subscribe<StrategicBettingStartedSignal>(OnSbStarted);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<InputDirectionSignal>(OnInput);
            _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.TryUnsubscribe<RevivePlayerSignal>(OnRevive);
            _signalBus.TryUnsubscribe<StrategicBettingStartedSignal>(OnSbStarted);
        }

        public void Tick()
        {
            if (_model.IsRespawning)
            {
                HandleRespawnTick();
                return;
            }

            if (!_gameIsRunning) return;

            _sbPressure.Tick(Time.deltaTime);

            HandlePowerUpTimers();
            HandleMovementTick();
        }

        private void HandleMovementTick()
        {
            _moveTimer += Time.deltaTime;
            if (_moveTimer < _model.MoveFrequency) return;

            _moveTimer = 0;

            if (_isSbMode && _model.Body.Count >= _targetLength)
            {
                WinSession();
                return;
            }

            if (!_engine.TickMovement(out var newHead))
            {
                HandleDeath("Collision");
                return;
            }

            _signalBus.Fire(new DistanceTravelled());

            var item = _itemSpawner.GetItemAt(newHead);
            if (item != null)
            {
                HandleItemInteraction(item, newHead);
            }
            else
            {
                _engine.RemoveTail();
            }
        }

        private void HandleItemInteraction(ActiveItem item, Vector2Int pos)
        {
            var data = item.Data;

            if (data.isCollectible)
            {
                HandleFoodEaten(data, pos);
            }
            else if (data.isObstacle)
            {
                if (!_model.IsInvulnerable) HandleDeath(data.type.ToString());
                else _engine.RemoveTail(); 
            }
            else if (data.isPowerUp)
            {
                ApplyPowerUp(data, pos);
                _signalBus.Fire(new PowerUpCollected());
                _engine.RemoveTail();
            }

            _itemSpawner.RemoveItem(pos);
            UnityEngine.Object.Destroy(item.Instance);
        }

        private void HandleFoodEaten(GameItem data, Vector2Int pos)
        {
            bool isPrecious = data.type == ItemType.PreciousFood;

            _sbPressure.ResetTimer();

            if (isPrecious)
            {
                _signalBus.Fire(new PreciousGemCollected());
                _view.PlayPreciousGemEffect();
            }
            else
            {
                _signalBus.Fire(new GemCollected());
                _view.PlayGemEffect();
            }

            if (isPrecious && _itemSpawner.HasActiveObstacles()) _signalBus.Fire(new TrapAvoided());

            int points = data.scoreValue * _model.Body.Count * 100;
            _score += points;

            _signalBus.Fire(new ScoreUpdatedSignal(_score));
            _signalBus.Fire(new ScoreAddedSignal(points, pos));
            _signalBus.Fire(new LengthUpdatedSignal(_model.Body.Count, _targetLength));
            _signalBus.Fire(new PlaySoundSignal(isPrecious ? SoundType.PreciousFoodCollect : SoundType.FoodCollect));

            _itemSpawner.OnFoodCollected();
        }

        private void ApplyPowerUp(GameItem data, Vector2Int pos)
        {
            _signalBus.Fire(new PlaySoundSignal(SoundType.SpeedUp));

            if (data.effectType == PowerUpEffectType.SpeedUp)
            {
                _powerUpTimer = data.effectDuration;
                _model.MoveFrequency = SpeedUpFrequency;
                _signalBus.Fire(new SnakeEffectSignal("Speed Up!", pos));
                _view.PlaySpeedUpEffect();
            }
            else if (data.effectType == PowerUpEffectType.Invulnerable)
            {
                _invulnerableTimer = data.effectDuration;
                _model.IsInvulnerable = true;
                _signalBus.Fire(new SnakeEffectSignal("Invulnerable!", pos));
                _view.PlayInvulnerabilityEffect();
            }
        }

        private void HandleDeath(string reason)
        {
            Debug.LogWarning(Time.time);

            _gameIsRunning = false;
            _signalBus.Fire(new PlaySoundSignal(SoundType.GameOver));
            _signalBus.Fire(new PlayerDiedSignal(reason));

            _view.PlayBoomEffect();
            _view.ToggleVisuals(false);

            _lives--;
            _signalBus.Fire(new LifeUpdatedSignal(_lives));

            if (_lives > 0) StartRespawnSequence();
            else _signalBus.Fire(new GameOverSignal(_score, _model.Body.Count, default));
        }

        private void StartRespawnSequence()
        {
            _engine.Reset();
            _model.IsRespawning = true;
            _respawnTimer = RespawnDelay;
        }

        private void HandleRespawnTick()
        {
            _respawnTimer -= Time.deltaTime;
            if (_respawnTimer <= 0)
            {
                _model.IsRespawning = false;
                _view.ToggleVisuals(true);
                _gameIsRunning = true;
                _signalBus.Fire(new SnakeEffectSignal("Go!", _model.Body[0]));

                // Grace period invulnerability
                _model.IsInvulnerable = true;
                _invulnerableTimer = InvulnerableAfterRespawn;
            }
        }

        private void HandlePowerUpTimers()
        {
            // Shield timer
            if (_model.IsInvulnerable)
            {
                _invulnerableTimer -= Time.deltaTime;
                if (_invulnerableTimer <= 0)
                {
                    _model.IsInvulnerable = false;
                    _signalBus.Fire(new SnakeEffectSignal("Shield Down", _model.Body[0]));
                }
            }

            if (Mathf.Abs(_model.MoveFrequency - BaseFrequency) > FloatTolerance)
            {
                _powerUpTimer -= Time.deltaTime;
                if (_powerUpTimer <= 0)
                {
                    _model.MoveFrequency = BaseFrequency;
                    _signalBus.Fire(new PlaySoundSignal(SoundType.SpeedDown));
                }
            }
        }

        private void WinSession()
        {
            _gameIsRunning = false;
            _signalBus.Fire(new LevelCompletedSignal()); 
            _signalBus.Fire(new GameOverSignal(_score, _model.Body.Count, default));
        }

        private void OnGameStarted() 
        {
            _gameIsRunning = true;
            ResetForNewSession();
        }
        private void OnSbStarted(StrategicBettingStartedSignal signal)
        {
            _isSbMode = true;
            _targetLength = signal.TargetLength;
            _lives = 1;
            _signalBus.Fire(new LifeUpdatedSignal(_lives));
        }

        private void ResetForNewSession()
        {
            _score = 0;
            _lives = StartingLives;
            _isSbMode = false;
            _model.MoveFrequency = BaseFrequency;
            _model.IsInvulnerable = false;
            _model.IsRespawning = false;
            _engine.Reset();

            _signalBus.Fire(new LifeUpdatedSignal(_lives));
            _signalBus.Fire(new ScoreUpdatedSignal(0));
        }

        private void OnRevive(RevivePlayerSignal signal)
        {
            _lives = StartingLives;
            _signalBus.Fire(new LifeUpdatedSignal(_lives));
            StartRespawnSequence();
        }

        private void OnInput(InputDirectionSignal signal) => _engine.SetInput(signal.Direction);
    }
}