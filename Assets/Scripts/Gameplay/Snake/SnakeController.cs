using System;
using Core.Enums;
using Core.Events;
using Core.Services;
using UnityEngine;
using Zenject;

namespace Gameplay.Snake
{
    public class SnakeGameController : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly SnakeEngine _engine;
        private readonly SnakeModel _model;
        private readonly ItemSpawner _itemSpawner;
        private readonly SnakeView _view;
        
        private const float BaseFrequency = 0.1f;
        private const float SpeedUpFrequency = 0.08f;
        private const float RespawnDelay = 0.4f;
        private const int StartingLives = 3;
        
        private float _moveTimer;
        private float _powerUpTimer;
        private float _respawnTimer;
        private bool _gameIsRunning;
        private int _lives;
        private int _score;
        
        public float InterpolationFactor => _gameIsRunning ? (_moveTimer / _model.MoveFrequency) : 0f;

        public SnakeGameController(
            SignalBus signalBus, 
            SnakeEngine engine, 
            SnakeModel model,
            ItemSpawner spawner,
            SnakeView view)
        {
            _signalBus = signalBus;
            _engine = engine;
            _model = model;
            _itemSpawner = spawner;
            _view = view;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<InputDirectionSignal>(OnInput);
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChange);
            _signalBus.Subscribe<RevivePlayerSignal>(OnRevive); 
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<InputDirectionSignal>(OnInput);
            _signalBus.TryUnsubscribe<GameStateChangedSignal>(OnStateChange);
            _signalBus.TryUnsubscribe<RevivePlayerSignal>(OnRevive);
        }

        private void OnStateChange(GameStateChangedSignal signal)
        {
            _gameIsRunning = (signal.NewState == GameState.InGame);
            
            if (signal.NewState == GameState.MainMenu)
            {
                ResetForNewSession();
            }
        }

        private void ResetForNewSession()
        {
            _score = 0;
            _lives = StartingLives;
            _signalBus.Fire(new LifeUpdatedSignal { LifeRemaining = _lives });
            _signalBus.Fire(new ScoreUpdatedSignal { TotalScore = 0 });

            _model.MoveFrequency = BaseFrequency;
            _model.IsInvulnerable = false;
            _model.IsRespawning = false;
            _engine.Reset();
        }

        public void Tick()
        {
            if (_model.IsRespawning)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0)
                {
                    _model.IsRespawning = false;
                    _view.ToggleVisuals(true);
                    _gameIsRunning = true;
                    _signalBus.Fire(new SnakeEffectSignal { EffectName = "Go!", Position = _model.Body[0] });
                }
                return; 
            }

            if (!_gameIsRunning) return;

            if (_model.MoveFrequency < BaseFrequency || _model.IsInvulnerable)
            {
                _powerUpTimer -= Time.deltaTime;
                if (_powerUpTimer <= 0)
                {
                    if (_model.MoveFrequency != BaseFrequency)
                    {
                        _signalBus.Fire(new PlaySoundSignal { Type = SoundType.SpeedDown });
                        _model.MoveFrequency = BaseFrequency;
                    }
                    _model.IsInvulnerable = false;
                }
            }

            _moveTimer += Time.deltaTime;
            if (_moveTimer >= _model.MoveFrequency)
            {
                _moveTimer = 0;
                PerformMovementStep();
            }
        }

        private void PerformMovementStep()
        {
            if (!_engine.TickMovement(out var newHead))
            {
                HandleDeath("Collision");
                return;
            }

            var item = _itemSpawner.GetItemAt(newHead);
            bool snakeGrew = false;

            if (item != null)
            {
                 snakeGrew = HandleItemInteraction(item, newHead);
            }

            if (!snakeGrew)
            {
                _engine.RemoveTail();
            }
            else
            {
                CheckGrowthAchievements();
            }
        }

        private bool HandleItemInteraction(ActiveItem item, Vector2Int pos)
        {
            var data = item.Data;
            
            if (data.isCollectible)
            {
                _model.GemsCollected++;
                CheckFoodAchievements();

                bool isPrecious = data.objName == "PreciousFood";
                int points = data.scoreValue * _model.Body.Count;
                _score += points;
                _signalBus.Fire(new ScoreUpdatedSignal { TotalScore = _score });
                _signalBus.Fire(new ScoreAddedSignal { Amount = points, Position = pos });
                
                if (isPrecious) 
                    _signalBus.Fire(new PreciousGemEatenSignal());
                
                SoundType sound = isPrecious ? SoundType.PreciousFoodCollect : SoundType.FoodCollect;
                _signalBus.Fire(new PlaySoundSignal { Type = sound });

                _itemSpawner.RemoveItem(pos); 
                UnityEngine.Object.Destroy(item.Instance);
                _itemSpawner.OnFoodCollected();
                
                return true; 
            }
            
            if (data.isObstacle && !_model.IsInvulnerable && !_model.IsRespawning)
            {
                HandleDeath(data.objName);
                return false;
            }

            if (data.isPowerUp)
            {
                ApplyPowerUp(data, pos);
                _itemSpawner.RemoveItem(pos);
                UnityEngine.Object.Destroy(item.Instance);
                return false;
            }

            return false;
        }

        private void ApplyPowerUp(GameItem data, Vector2Int pos)
        {
            _signalBus.Fire(new PlaySoundSignal { Type = SoundType.SpeedUp });
            _powerUpTimer = data.effectDuration;

            if (data.effectType == PowerUpEffectType.SpeedUp)
            {
                _model.MoveFrequency = SpeedUpFrequency;
                _model.SpeedUpsCollected++;
                CheckSpeedAchievements();
                _signalBus.Fire(new SnakeEffectSignal { EffectName = "Speed Up!", Position = pos });
            }
            else if (data.effectType == PowerUpEffectType.Invulnerable)
            {
                _model.IsInvulnerable = true;
                _signalBus.Fire(new SnakeEffectSignal { EffectName = "Invulnerable!", Position = pos });
            }
        }

        private void HandleDeath(string reason)
        {
            _signalBus.Fire(new PlaySoundSignal { Type = SoundType.GameOver });
            _signalBus.Fire(new PlayerDiedSignal { DeathReason = reason });
            
            _gameIsRunning = false;
                     
            _view.PlayBoomEffect();
            _view.ToggleVisuals(false);
            _lives--;
            _signalBus.Fire(new LifeUpdatedSignal { LifeRemaining = _lives });

            if (_lives > 0)
            {
                StartRespawnSequence();
            }
            else
            {
                _signalBus.Fire(new GameOverSignal { FinalScore = _score });
            }
        }

        private void StartRespawnSequence()
        {
            _engine.Reset(); 
            _model.IsRespawning = true;
            _respawnTimer = RespawnDelay;
        }
        
        private void OnRevive(RevivePlayerSignal signal)
        {
            _lives = 3; 
            _signalBus.Fire(new LifeUpdatedSignal { LifeRemaining = _lives });
            StartRespawnSequence(); 
        }

        private void OnInput(InputDirectionSignal signal)
        {
            _engine.SetInput(signal.Direction);
        }

        private void CheckFoodAchievements()
        {
            int c = _model.GemsCollected;
            if (c == 40) FireAch("FoodC_xx");
            else if (c == 25) FireAch("FoodC_x");
            else if (c == 15) FireAch("FoodC_m");
        }

        private void CheckGrowthAchievements()
        {
            int count = _model.Body.Count;
            if (count == 60) FireAch("Size_xxl");
            else if (count == 40) FireAch("Size_xx");
            else if (count == 25) FireAch("Size_x");
        }
        
        private void CheckSpeedAchievements()
        {
            int c = _model.SpeedUpsCollected;
            if (c == 40) FireAch("SpeedC_xxl");
            else if (c == 25) FireAch("SpeedC_xx");
            else if (c == 10) FireAch("SpeedC_x");
            else if (c == 3) FireAch("SpeedC_m");
        }

        private void FireAch(string id) => _signalBus.Fire(new AchievementProgressSignal { AchievementId = id });
    }
}