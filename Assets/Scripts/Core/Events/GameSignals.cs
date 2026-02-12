using Core.Enums;
using Core.Services;
using UnityEngine;

namespace Core.Events
{
    public struct GameStateChangedSignal { public GameState NewState; }
    public struct GameStartedSignal { } 
    public struct GameOverSignal { public int FinalScore; } 
    public struct RevivePlayerSignal { } 
    
    public struct InputDirectionSignal { public Vector2Int Direction; }
    public struct ScoreUpdatedSignal { public int TotalScore;}
    public struct ScoreAddedSignal { public int Amount; public Vector2 Position; }
    
    public struct LifeUpdatedSignal { public int LifeRemaining; }
    public struct PlayerDiedSignal { public string DeathReason; }
    public struct PreciousGemEatenSignal { }
    public struct ItemDestroyedSignal { public Vector2Int ItemPosition; }
    
    public struct PlaySoundSignal { public SoundType Type; }
    public struct InactivityTimerSignal { public int SecondsLeft; }
    public struct InactivityTimeOut {}
    
    public struct AchievementProgressSignal { public string AchievementId; } 
    public struct SnakeEffectSignal { public string EffectName; public Vector2 Position; }
    
    public struct ErrorSignal { public int Code; public string Message; }
}

