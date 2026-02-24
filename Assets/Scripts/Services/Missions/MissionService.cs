using System.Collections.Generic;
using System.Linq;
using Luxodd.Game.Scripts.Missions; // Using the native Luxodd models
using Services.Gameloop;
using UnityEngine;

namespace Services.Missions
{
    public class MissionEvaluatorService
    {
        private readonly List<MissionData> _missionDatabase;
        private StrategicBettingData _currentSessionData;
        private bool _isStrategicBettingActive;

        public MissionEvaluatorService(List<MissionData> missionDatabase)
        {
            _missionDatabase = missionDatabase;
        }

        public void SetupSession(string sessionType, StrategicBettingData sessionData)
        {
            if (sessionType == "sb" && sessionData != null)
            {
                _isStrategicBettingActive = true;
                _currentSessionData = sessionData;
            }
            else
            {
                _isStrategicBettingActive = false;
                _currentSessionData = null;
            }
        }

        public bool IsStrategicBettingActive()
        {
            return _isStrategicBettingActive;
        }

        public List<MissionResultDto> EvaluateMissions(GameSessionStats finalStats)
        {
            var results = new List<MissionResultDto>();

            if (!_isStrategicBettingActive || _currentSessionData?.Missions == null) 
                return results;

            foreach (var activeMission in _currentSessionData.Missions)
            {
                var missionDef = _missionDatabase.FirstOrDefault(m => m.Id == activeMission.MissionId);
                
                if (missionDef == null)
                {
                    Debug.LogWarning($"Mission {activeMission.MissionId} not found in local database!");
                    continue;
                }

                bool isCompleted = CheckMissionCondition(missionDef, finalStats);
                
                results.Add(new MissionResultDto
                {
                    MissionId = activeMission.MissionId,
                    Outcome = isCompleted ? "win" : "loss"
                });
            }

            return results;
        }

        private bool CheckMissionCondition(MissionData mission, GameSessionStats stats)
        {
            switch (mission.Id)
            {
                case "mission_collect_food":
                    return stats.GemsCollected >= mission.Value;
                
                case "mission_collect_precious":
                    return stats.PreciousGemsCollected >= mission.Value;
                
                case "mission_avoid_traps":
                    return stats.TrapsAvoided >= mission.Value;
                
                case "mission_distance":
                    return stats.DistanceTravelled >= mission.Value;
                
                default:
                    return false;
            }
        }
    }
}