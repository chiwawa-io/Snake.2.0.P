using System;
using System.Collections.Generic;
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
        private NetworkManager _networkManager;
        private SignalBus _signalBus;

        private int _bestScore;
        private int _currentXp;
        private int _currentLevel;
        private HashSet<string> _completedAchievementIds = new();

        public event Action OnDataLoaded;
        public event Action OnDataSaved;

        public int GetBestScore() => _bestScore;
        public int GetLevel() => _currentLevel;
        public float GetExpNormalized() => (_currentXp / 1000f);

        [Inject]
        public void Construct(NetworkManager networkManager, IBackendService backendService, SignalBus signalBus)
        {
            _networkManager = networkManager;
            _signalBus = signalBus;
            
            LoadData();
        }

        private void LoadData()
        {
            _networkManager.WebSocketCommandHandler.SendGetUserDataRequestCommand(OnDataLoadSuccess, OnDataLoadError);
        }

        public void SaveGameSession(int score)
        {
            if (score > _bestScore) _bestScore = score;

            int xpGained = score / 10; 
            AddExperience(xpGained);

            SaveDataInternal();
        }

        private void AddExperience(int amount)
        {
            _currentXp += amount;
            while (_currentXp >= 1000)
            {
                _currentXp -= 1000;
                _currentLevel++;
            }
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
            if (_completedAchievementIds.Contains(achievementId))
                return true;
            return false;
        }

        private void SaveDataInternal()
        {
            var data = new PlayerData(_bestScore, _currentLevel, _currentXp, _completedAchievementIds);
            _networkManager.WebSocketCommandHandler.SendSetUserDataRequestCommand(data, 
                () => {
                    OnDataSaved?.Invoke(); 
                    Debug.Log("Data Sync Success");
                }, 
                (code, msg) => _signalBus.Fire(new ErrorSignal{Code = code, Message = msg})
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

            OnDataLoaded?.Invoke();
        }

        private void OnDataLoadError(int code, string msg)
        {
            InitEmpty();
            _signalBus.Fire(new ErrorSignal { Code = code, Message = msg });
        }

        private void InitEmpty()
        {
            _bestScore = 0;
            _currentLevel = 1;
            _currentXp = 0;
            _completedAchievementIds = new HashSet<string>();
        }
    }
}
