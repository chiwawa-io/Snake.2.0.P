using Core.Enums;
using Core.Services;
using UnityEngine;

namespace Core.Events
{
    //GameFlow
    public struct GameStartedSignal { } 
    public struct RevivePlayerSignal { }

    public readonly struct GameStateChangedSignal
    {
        public readonly GameState NewState;
        public GameStateChangedSignal (GameState newState) => NewState = newState;
    }

    public readonly struct GameOverSignal
    {
        public readonly int FinalScore; 
        public GameOverSignal(int finalScore) => FinalScore = finalScore;
    }

    public readonly struct InputDirectionSignal
    {
        public readonly Vector2Int Direction; 
        public InputDirectionSignal(Vector2Int direction) => Direction = direction;
        
    }

    public readonly struct ScoreUpdatedSignal
    {
        public readonly int TotalScore;
        public ScoreUpdatedSignal(int totalScore) => TotalScore = totalScore;
    }

    public readonly struct ScoreAddedSignal
    {
        public readonly int Amount; 
        public readonly Vector2 Position;

        public ScoreAddedSignal(int amount, Vector2 position)
        {
            Amount = amount;
            Position = position;
        }
    }

    public readonly struct ExpAddedSignal
    {
        public readonly int Amount;
        public ExpAddedSignal(int amount) => Amount = amount;
    }
    
    //Snake
    public struct PreciousGemEatenSignal { }

    public readonly struct LifeUpdatedSignal
    {
        public readonly int LifeRemaining; 
        public LifeUpdatedSignal(int lifeRemaining) => LifeRemaining = lifeRemaining;
    }

    public readonly struct PlayerDiedSignal
    {
        public readonly string DeathReason; 
        public PlayerDiedSignal(string deathReason) => DeathReason = deathReason; 
    }

    public readonly struct ItemDestroyedSignal
    {
        public readonly Vector2Int ItemPosition; 
        public ItemDestroyedSignal(Vector2Int itemPosition) => ItemPosition = itemPosition;
    }

    public readonly struct PlaySoundSignal
    {
        public readonly SoundType Type; 
        public PlaySoundSignal(SoundType type) => Type = type;
    }

    public readonly struct SnakeEffectSignal
    {
        public readonly string EffectName; 
        public readonly Vector2 Position; 
        public SnakeEffectSignal(string effectName, Vector2 position)
        {
            EffectName = effectName;
            Position = position;
        }
    }
    
    //Timer
    public struct InactivityTimeOut {}

    public readonly struct InactivityTimerSignal
    {
        public readonly int SecondsLeft; 
        public InactivityTimerSignal(int secondsLeft) => SecondsLeft = secondsLeft;
    }
    //Other
    public readonly struct AchievementProgressSignal
    {
        public readonly string AchievementId; 
        public AchievementProgressSignal(string achievementId) => AchievementId = achievementId;
    }

    public readonly struct ErrorSignal
    {
        public readonly int Code; 
        public readonly string Message;
        public ErrorSignal(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}

