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
        
        private const float BaseFrequency = 0.1f;
        private const float SpeedUpFrequency = 0.05f;
        
        private float _moveTimer;
        private float _powerUpTimer;
        private float _respawnTimer;
        private bool _gameIsRunning;
        
        private int _lives = 3;
        private int _score;

        public SnakeGameController(
            SignalBus signalBus, 
            SnakeEngine engine, 
            SnakeModel model,
            ItemSpawner spawner)
        {
            _signalBus = signalBus;
            _engine = engine;
            _model = model;
            _itemSpawner = spawner;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<InputDirectionSignal>(OnInput);
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChange);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<InputDirectionSignal>(OnInput);
            _signalBus.Unsubscribe<GameStateChangedSignal>(OnStateChange);
        }

        private void OnStateChange(GameStateChangedSignal signal)
        {
            _gameIsRunning = (signal.NewState == GameState.InGame);
            
            if (signal.NewState == GameState.MainMenu)
            {
                ResetGame();
            }
        }

        private void ResetGame()
        {
            _model.MoveFrequency = BaseFrequency;
            _engine.Reset();
            // Trigger Spawner Reset if needed
            // _itemSpawner.Initialize(...); // Do this in a factory or specific init method
        }

        public void Tick()
        {
            if (!_gameIsRunning) return;

            float dt = Time.deltaTime;

            // 1. Handle Respawn Timer
            if (_model.IsRespawning)
            {
                _respawnTimer -= dt;
                if (_respawnTimer <= 0)
                {
                    _model.IsRespawning = false;
                    _signalBus.Fire(new SnakeEffectSignal { EffectName = "Go!", Position = _model.Body[0] });
                }
                return; // Don't move while respawning
            }

            // 2. Handle PowerUp Timers
            if (_model.MoveFrequency < BaseFrequency || _model.IsInvulnerable)
            {
                _powerUpTimer -= dt;
                if (_powerUpTimer <= 0)
                {
                    // Reset Effects
                    if (_model.MoveFrequency != BaseFrequency)
                    {
                        _signalBus.Fire(new PlaySoundSignal { Type = SoundType.SpeedDown });
                        _model.MoveFrequency = BaseFrequency;
                    }
                    _model.IsInvulnerable = false;
                }
            }

            // 3. Movement Timer
            _moveTimer += dt;
            if (_moveTimer >= _model.MoveFrequency)
            {
                _moveTimer = 0;
                PerformMovementStep();
            }
        }

        // This method replaces "Move()" in Player.cs
        private void PerformMovementStep()
        {
            Vector2Int newHead;
            
            if (!_engine.TickMovement(out newHead))
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
                
                var points = data.scoreValue * _model.Body.Count;
                _score += points;
                _signalBus.Fire(new ScoreAddedSignal {Amount = points, Position = pos}); 
                
                SoundType sound = (data.objName == "PreciousFood") ? SoundType.PreciousFoodCollect : SoundType.FoodCollect;
                _signalBus.Fire(new PlaySoundSignal { Type = sound });

                _itemSpawner.RemoveItem(pos); 
                UnityEngine.Object.Destroy(item.Instance); // Visual destroy
                _itemSpawner.OnFoodCollected(); // Spawn new
                return true; 
            }
            else if (data.isObstacle)
            {
                 if (!_model.IsInvulnerable && !_model.IsRespawning)
                 {
                     HandleDeath(data.objName);
                 }
                 return false;
            }
            else if (data.isPowerUp)
            {
                ApplyPowerUp(data, pos);
                _itemSpawner.RemoveItem(pos);
                UnityEngine.Object.Destroy(item.Instance);
                return false; // Didn't grow
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
            
            _lives--;
            
            if (_lives > 3)
                _signalBus.Fire(new LifeUpdatedSignal { LifeRemaining = _lives });
            else
            {
                _signalBus.Fire(new GameOverSignal {FinalScore = _score});
            }
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

