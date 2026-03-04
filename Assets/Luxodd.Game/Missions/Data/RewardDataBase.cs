using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    [CreateAssetMenu(menuName = "Create/Reward/Reward Data Base", fileName = "RewardDataBase", order = 0)]
    public class RewardDataBase : ScriptableObject
    {
        [SerializeField] private List<RewardBundle> _rewards = new List<RewardBundle>();

        public RewardBundle GetReward(int id)
        {
            var result = _rewards.Find(bundle => bundle.Id == id);
            return result;
        }

        public RewardBundle GetReward(string bundleName)
        {
            var result = _rewards.Find(bundle => bundle.Name == bundleName);
            return result;
        }
    }
    
    
    [Serializable]
    public class RewardItemData
    {
        [field: SerializeField] public ItemType Type { get; private set; }
        [field: SerializeField] public int Amount { get; private set; }
    }
}
