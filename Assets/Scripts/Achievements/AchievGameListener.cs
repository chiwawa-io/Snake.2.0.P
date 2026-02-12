using System;
using System.Collections.Generic;
using Core.Events;
using Services.PlayerData;
using UnityEngine;
using Zenject;



public class AchievGameListener : MonoBehaviour
{
    [SerializeField] private List<AchievementSO> achievementsList = new();
    
    private PlayerDataManager _playerDataManager;
    private SignalBus _signalBus;
    
    private readonly List<string> _completedAchievements = new();
    
    public static Action<string> OnAchievementCompleted;

    [Inject]
    public void Construct(PlayerDataManager playerDataManager, SignalBus signalBus)
    {
        _signalBus = signalBus;
        _playerDataManager = playerDataManager;
    }

    private void OnEnable()
    {
        _signalBus.Subscribe<AchievementProgressSignal>(AchievementComplete);
        LoadCompletedAchievements();
    }

    private void OnDisable()
    {
        _signalBus.Unsubscribe<AchievementProgressSignal>(AchievementComplete);
    }

    private void LoadCompletedAchievements()
    {
        foreach (var achievement in achievementsList)
        {
            if (_playerDataManager.IsAchievementCompleted(achievement.id))
                _completedAchievements.Add(achievement.id);
        }
    }

    private void AchievementComplete(AchievementProgressSignal achievement)
    {
        _playerDataManager.UnlockAchievement(achievement.AchievementId);

        var achievementData = GetAchievementById(achievement.AchievementId);
        
        if (achievementData != null && !_completedAchievements.Contains(achievement.AchievementId)) OnAchievementCompleted?.Invoke(achievementData.displayName);
        
        if (!_completedAchievements.Contains(achievement.AchievementId)) _completedAchievements.Add(achievement.AchievementId);
    }

    private AchievementSO GetAchievementById(string id)
    {
        foreach (var achievementData in achievementsList)
        {
            if (achievementData.id == id) return achievementData;
            
            Debug.LogWarning("Not found the Achievement");
        }
        return null;
    }
}
