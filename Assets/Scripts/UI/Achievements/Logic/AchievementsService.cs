using System.Collections.Generic;
using Services.PlayerData;
using UnityEngine;

namespace UI.Achievements.Logic
{
    public class AchievementService
    {
        private readonly List<AchievementSO> _allAchievements;
        private readonly PlayerDataManager _playerData; 
        
        public int ItemsPerPage { get; set; } = 3;

        public AchievementService(List<AchievementSO> assets, PlayerDataManager playerData)
        {
            _allAchievements = assets;
            _playerData = playerData;
        }

        public int GetTotalPages()
        {
            return Mathf.CeilToInt((float)_allAchievements.Count / ItemsPerPage);
        }

        public List<AchievementDisplayData> GetPage(int pageIndex)
        {
            var result = new List<AchievementDisplayData>();
            
            int start = pageIndex * ItemsPerPage;
            int end = Mathf.Min(start + ItemsPerPage, _allAchievements.Count);

            for (int i = start; i < end; i++)
            {
                var data = _allAchievements[i];
                bool isUnlocked = _playerData.IsAchievementCompleted(data.id);
                
                result.Add(new AchievementDisplayData 
                { 
                    Name = data.name, 
                    Description = data.description, 
                    IsUnlocked = isUnlocked
                });
            }
            return result;
        }
    }

    public struct AchievementDisplayData
    {
        public string Name;
        public string Description;
        public bool IsUnlocked;
    }
}

