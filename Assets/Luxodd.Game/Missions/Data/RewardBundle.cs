using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    [CreateAssetMenu(menuName = "Create/Reward/Reward Bundle", fileName = "RewardBundle", order = 0)]
    [Serializable]
    public class RewardBundle : ScriptableObject
    {
        [field: SerializeField] public List<RewardItemData> Rewards { get; private set; }
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
    }
}