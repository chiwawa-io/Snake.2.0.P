using Core.Enums;
using Services.Audio;
using Services.Gameloop;
using UnityEngine;

namespace Core.Events
{
    //GameFlow
    public readonly struct GameStartedSignal { } 
    public readonly struct RevivePlayerSignal { }

    public readonly struct GameStateChangedSignal
    {
        public readonly GameState NewState;
        public GameStateChangedSignal (GameState newState) => NewState = newState;
    }

    public readonly struct LevelCompletedSignal { }
    public readonly struct UpdateMissionProgressSignal 
    { 
        public readonly int CurrentValue; 
        public UpdateMissionProgressSignal(int currentValue) => CurrentValue = currentValue;
    }
    public readonly struct GameOverSignal
    {
        public readonly int FinalScore; 
        public readonly int FinalLength;
        public readonly GameSessionStats Stats;
        public GameOverSignal(int finalScore, int finalLength, GameSessionStats stats)
        {
            FinalScore = finalScore;
            FinalLength = finalLength;
            Stats = stats;
        } 
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
    public readonly struct LengthUpdatedSignal 
    { 
        public readonly int CurrentLength; 
        public readonly int TargetLength;

        public LengthUpdatedSignal(int current, int target) 
        { 
            CurrentLength = current; 
            TargetLength = target; 
        }
    }
    
    public readonly struct GrowthTimerUpdatedSignal 
    { 
        public readonly float TimeRemaining; 
        public readonly float TotalTime;
        public GrowthTimerUpdatedSignal(float remaining, float total) 
        {
            TimeRemaining = remaining; 
            TotalTime = total; 
        }
    }
    public readonly struct StrategicBettingStartedSignal 
    { 
        public readonly int TargetLength; 
        public readonly int Hardness;

        public StrategicBettingStartedSignal(int target, int hardness)
        {
            TargetLength = target; 
            Hardness = hardness;
        }
    }
    
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
    
    //Stats
    public readonly struct GemCollected{}
    public readonly struct PowerUpCollected{}
    public readonly struct PreciousGemCollected{}
    public readonly struct TrapAvoided {}
    public readonly struct DistanceTravelled {}

    
    //Timer
    public readonly struct InactivityTimeOut {}
    public readonly struct InactivityTimerSignal
    {
        public readonly int SecondsLeft; 
        public InactivityTimerSignal(int secondsLeft) => SecondsLeft = secondsLeft;
    }
    //Other
    public readonly struct AchievementProgressSignal
    {
        public readonly string AchievementId; 
        public readonly string AchievementName;

        public AchievementProgressSignal(string achievementId, string achievementName)
        {
            AchievementId = achievementId;
            AchievementName = achievementName;
        }
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

