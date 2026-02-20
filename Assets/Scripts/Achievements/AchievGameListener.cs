using System;
using System.Collections.Generic;
using Achievements.Data;
using Core.Events;
using Services.PlayerData;
using UnityEngine;
using Zenject;

namespace Achievements
{

    public class AchievGameListener : MonoBehaviour
    {
        public static Action<string> OnAchievementCompleted;

        [SerializeField] private List<AchievementSO> _achievementsList = new();

        private PlayerDataManager _playerDataManager;
        private SignalBus _signalBus;
        private readonly List<string> _completedAchievements = new();

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
            foreach (var achievement in _achievementsList)
            {
                if (_playerDataManager.IsAchievementCompleted(achievement.id))
                    _completedAchievements.Add(achievement.id);
            }
        }

        private void AchievementComplete(AchievementProgressSignal achievement)
        {
            _playerDataManager.UnlockAchievement(achievement.AchievementId);

            var achievementData = GetAchievementById(achievement.AchievementId);

            if (achievementData != null && !_completedAchievements.Contains(achievement.AchievementId))
                OnAchievementCompleted?.Invoke(achievementData.displayName);

            if (!_completedAchievements.Contains(achievement.AchievementId))
                _completedAchievements.Add(achievement.AchievementId);
        }

        private AchievementSO GetAchievementById(string id)
        {
            foreach (var achievementData in _achievementsList)
            {
                if (achievementData.displayName == id) return achievementData;

                Debug.Log("Not found the Achievement");
            }

            return null;
        }
    }
}