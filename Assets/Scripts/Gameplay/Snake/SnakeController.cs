using System;
using System.Collections.Generic;
using Achievements.Data;
using Core.Enums;
using Core.Events;
using Gameplay.GameItem;
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
        private const int StartingLives = 3;
        private const float FloatTolerance = 0.001f;

        private readonly SignalBus _signalBus;
        private readonly SnakeEngine _engine;
        private readonly SnakeModel _model;
        private readonly ItemSpawner _itemSpawner;
        private readonly SnakeView _view;
        private readonly AchievementCompletion _completionConfig;
        
        private float _moveTimer;
        private float _powerUpTimer;
        private float _respawnTimer;
        private bool _gameIsRunning;
        private int _lives;
        private int _score;
        
        public float InterpolationFactor => _gameIsRunning ? (_moveTimer / _model.MoveFrequency) : 0f;

        public SnakeController(
            SignalBus signalBus, 
            SnakeEngine engine, 
            SnakeModel model,
            ItemSpawner spawner,
            SnakeView view,
            AchievementCompletion completionConfig)
        {
            _signalBus = signalBus;
            _engine = engine;
            _model = model;
            _itemSpawner = spawner;
            _view = view;
            _completionConfig = completionConfig;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<InputDirectionSignal>(OnInput);
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChange);
            _signalBus.Subscribe<RevivePlayerSignal>(OnRevive); 
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
                    _signalBus.Fire(new SnakeEffectSignal("Go!", _model.Body[0]));
                }
                return; 
            }

            if (!_gameIsRunning) return;

            if (_model.MoveFrequency < BaseFrequency || _model.IsInvulnerable)
            {
                _powerUpTimer -= Time.deltaTime;
                if (_powerUpTimer <= 0)
                {
                    if (Mathf.Abs(_model.MoveFrequency - BaseFrequency) > FloatTolerance)
                    {
                        _signalBus.Fire(new PlaySoundSignal(SoundType.SpeedDown));
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
            _signalBus.Fire(new LifeUpdatedSignal(_lives));
            _signalBus.Fire(new ScoreUpdatedSignal(0));

            _model.MoveFrequency = BaseFrequency;
            _model.IsInvulnerable = false;
            _model.IsRespawning = false;
            _engine.Reset();
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
                int points = data.scoreValue * _model.Body.Count * 100;
                _score += points;
                _signalBus.Fire(new ScoreUpdatedSignal(_score));
                _signalBus.Fire(new ScoreAddedSignal(points, pos));
                
                if (isPrecious) 
                    _signalBus.Fire(new PreciousGemEatenSignal());
                
                SoundType sound = isPrecious ? SoundType.PreciousFoodCollect : SoundType.FoodCollect;
                _signalBus.Fire(new PlaySoundSignal(sound));

                _itemSpawner.RemoveItem(pos); 
                UnityEngine.Object.Destroy(item.Instance);
                _itemSpawner.OnFoodCollected();
                
                return true; 
            }
            
            if (data.isObstacle && !_model.IsInvulnerable && !_model.IsRespawning)
            {
                HandleDeath(data.objName);
            }

            if (data.isPowerUp)
            {
                ApplyPowerUp(data, pos);
            }

            _itemSpawner.RemoveItem(pos);
            UnityEngine.Object.Destroy(item.Instance);
            return false;
        }

        private void ApplyPowerUp(GameItem.GameItem data, Vector2Int pos)
        {
            _signalBus.Fire(new PlaySoundSignal (SoundType.SpeedUp) );
            _powerUpTimer = data.effectDuration;

            if (data.effectType == PowerUpEffectType.SpeedUp)
            {
                _model.MoveFrequency = SpeedUpFrequency;
                _model.SpeedUpsCollected++;
                CheckSpeedAchievements();
                _signalBus.Fire(new SnakeEffectSignal("Speed Up!", pos));
            }
            else if (data.effectType == PowerUpEffectType.Invulnerable)
            {
                _model.IsInvulnerable = true;
                _signalBus.Fire(new SnakeEffectSignal("Invulnerable!", pos));
            }
        }

        private void HandleDeath(string reason)
        {
            _signalBus.Fire(new PlaySoundSignal (SoundType.GameOver));
            _signalBus.Fire(new PlayerDiedSignal(reason));
            
            _gameIsRunning = false;
                     
            _view.PlayBoomEffect();
            _view.ToggleVisuals(false);
            _lives--;
            _signalBus.Fire(new LifeUpdatedSignal(_lives));

            if (_lives > 0)
            {
                StartRespawnSequence();
            }
            else
            {
                _signalBus.Fire(new GameOverSignal (_score));
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
            _signalBus.Fire(new LifeUpdatedSignal(_lives));
            StartRespawnSequence(); 
        }

        private void OnInput(InputDirectionSignal signal)
        {
            _engine.SetInput(signal.Direction);
        }

        private void CheckFoodAchievements()
        {
            CheckAchievements(_completionConfig.FoodRules, _model.GemsCollected);
        }

        private void CheckGrowthAchievements()
        {
            CheckAchievements(_completionConfig.SnakeSizeRules, _model.Body.Count);
        }
    
        private void CheckSpeedAchievements()
        {
            CheckAchievements(_completionConfig.SpeedPowerUpRules, _model.SpeedUpsCollected);
        }
        
        private void CheckAchievements(List<AchievementCompletion.AchievementRule> rules, int currentValue)
        {
            foreach (var rule in rules)
            {
                if (currentValue == rule.Threshold)
                {
                    FireAch(rule.AchievementId);
                    break; 
                }
            }
        }

        private void FireAch(string id) => _signalBus.Fire(new AchievementProgressSignal(id));
    }
}