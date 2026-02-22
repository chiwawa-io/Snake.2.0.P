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
        [SerializeField] private List<AchievementSO> _achievementsList = new();

        private PlayerDataManager _playerDataManager;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(PlayerDataManager playerDataManager, SignalBus signalBus)
        {
            _signalBus = signalBus;
            _playerDataManager = playerDataManager;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<AchievementProgressSignal>(OnAchievementProgress);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<AchievementProgressSignal>(OnAchievementProgress);
        }

        private void OnAchievementProgress(AchievementProgressSignal signal)
        {
            if (_playerDataManager.IsAchievementCompleted(signal.AchievementId)) 
                return;

            _playerDataManager.UnlockAchievement(signal.AchievementId);
        }
    }
}