using System.Collections.Generic;
using Luxodd.Game.Scripts.Game.Leaderboard;
using UnityEngine;

namespace Game.UI
{
    public class LeaderboardView : BaseView
    {
        [SerializeField] private List<LeaderboardRow> entrySlots;

        public void Populate(List<LeaderboardData> data)
        {
            for (int i = 0; i < entrySlots.Count; i++)
            {
                if (data != null && i < data.Count)
                {
                    var player = data[i];
                    
                    entrySlots[i].SetData(player.Rank, player.PlayerName, player.TotalScore);
                }
                else
                {
                    entrySlots[i].SetEmpty();
                }
            }
        }
    }
}

