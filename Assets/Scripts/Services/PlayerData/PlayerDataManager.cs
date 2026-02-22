using System.Collections.Generic;
using Core.Enums;
using Core.Events;
using UnityEngine;
using Luxodd.Game.Scripts.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Services.Backend;
using Zenject;

namespace Services.PlayerData
{
    public class PlayerDataManager : MonoBehaviour
    {
        private const float ScoreMultiplicator = 0.01f;
        private const int LevelGainThreshold = 1000;
        private const int InitLevel = 1;
        
        private NetworkManager _networkManager;
        private SignalBus _signalBus;
        private int _bestScore;
        private int _currentXp;
        private int _currentLevel;
        private HashSet<string> _completedAchievementIds = new();

        [Inject]
        public void Construct(NetworkManager networkManager, IBackendService backendService, SignalBus signalBus)
        {
            _networkManager = networkManager;
            _signalBus = signalBus;
        }
        
        public int GetLevel() => _currentLevel;

        public float GetExpNormalized() => _currentXp;

        public void LoadData()
        {
            _networkManager.WebSocketCommandHandler.SendGetUserDataRequestCommand(OnDataLoadSuccess, OnDataLoadError);
        }

        public void SaveGameSession(int score)
        {
            if (score > _bestScore) _bestScore = score;

            var xpGained = score * ScoreMultiplicator; 
            AddExperience((int)xpGained);

            SaveDataInternal();
        }

        public void UnlockAchievement(string achievementId)
        {
            if (_completedAchievementIds.Add(achievementId))
            {
                SaveDataInternal();
            }
        }

        public bool IsAchievementCompleted(string achievementId)
        {
            return _completedAchievementIds.Contains(achievementId);
        }

        private void AddExperience(int amount)
        {
            _currentXp += amount;
            while (_currentXp >= LevelGainThreshold)
            {
                _currentXp -= LevelGainThreshold;
                _currentLevel++;
            }
        }

        private void SaveDataInternal()
        {
            var data = new PlayerData(_bestScore, _currentLevel, _currentXp, _completedAchievementIds);
            _networkManager.WebSocketCommandHandler.SendSetUserDataRequestCommand(data, 
                () => {
                    Debug.Log("Data Sync Success");
                }, 
                (code, msg) => _signalBus.Fire(new ErrorSignal(code, msg))
            );
        }

        private void OnDataLoadSuccess(object response)
        {
            try 
            {
                var payload = (UserDataPayload)response;
                var json = (JObject)payload.Data;
                
                if (json != null && json["user_data"] != null)
                {
                    var loaded = JsonConvert.DeserializeObject<PlayerData>(json["user_data"].ToString());
                    _bestScore = loaded.BestScore;
                    _currentLevel = loaded.Level;
                    _currentXp = loaded.Xp;
                    _completedAchievementIds = loaded.CompletedAchievementIds ?? new HashSet<string>();
                    Debug.LogWarning("Successfull loading player data!");
                }
                else
                {
                    InitEmpty();
                }
            }
            catch
            {
                InitEmpty();
            }

            _signalBus.Fire(new GameStateChangedSignal(GameState.MainMenu));
        }

        private void OnDataLoadError(int code, string msg)
        {
            InitEmpty();
            _signalBus.Fire(new ErrorSignal(code, msg));
        }

        private void InitEmpty()
        {
            _bestScore = 0;
            _currentXp = 0;
            _currentLevel = InitLevel;
            _completedAchievementIds = new HashSet<string>();
        }
    }
}