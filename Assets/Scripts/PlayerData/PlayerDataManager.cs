using System;
using System.Collections.Generic;
using Luxodd.Game.Scripts.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private int _bestScore;
    private int _currentXp;
    private int _currentLevel;
    //private int _skinShards;
    //private string _currentSkin;
    
    public int BestScore => _bestScore;
    
    private HashSet<string> _completedAchievementIds = new();
    //private HashSet<string> _ownedSkinIds = new();
    
    [SerializeField] private NetworkManager networkManager;

    public Action DataSaveSuccess; 
    

    public void LoadData()
    {
        networkManager.WebSocketCommandHandler.SendGetUserDataRequestCommand(OnDataLoadSuccess, OnDataLoadError);
    }

    public void Save(int score)
    {
        if (score > _bestScore)
            _bestScore = score;
        
        SaveData();
    }

    public void UpdateScore(int score)
    {
        if (score > _bestScore)
            _bestScore = score;
    }

    public bool IsAchievementCompleted(string achievementId)
    {
        return _completedAchievementIds.Contains(achievementId);
    }
    /*public bool IsSkinOwned(string achievementId)
    {
        return _ownedSkinIds.Contains(achievementId);
    }*/

    public void CompleteAchievement(string achievementId)
    {
        if (_completedAchievementIds.Add(achievementId))
        {
            //_skinShards += 5;
            // SaveData();
        }
    }
    /*public void BuySkin(string id)
    {
        if (_ownedSkinIds.Add(id))
        {
            SaveData();
        }
    }*/

    /*public void SetSkin(string skin)
    {
        _currentSkin = skin;
        SaveData();
    }
    
    public string GetCurrentSkin() => _currentSkin;
    public int GetShardsNum() => _skinShards;*/
    public int GetExp() => _currentXp;
    public int GetLevel() => _currentLevel;
    //public void Buy(int cost) => _skinShards -= cost;
    
    public void AddExperience(int experience)
    {
        _currentXp += experience;
        
        if (_currentXp < 1000)
            return;
            
        _currentXp -= 1000;
        _currentLevel++;
            
        /*if (_currentLevel >= 5) BuySkin("HellFire");
        if (_currentLevel >= 10) BuySkin("AtomicBreak");*/
    }

    private void SaveData()
    {
        Debug.LogWarning("SaveData started");
        
        var newPlayerData = new PlayerData(_bestScore, _currentLevel, _currentXp, _completedAchievementIds/*,  _ownedSkinIds, _currentSkin, _skinShards*/);
        
        networkManager.WebSocketCommandHandler.SendSetUserDataRequestCommand(newPlayerData, OnDataSaveSuccess, OnDataSaveError);    
    }

    private void OnDataSaveSuccess()
    {
        DataSaveSuccess?.Invoke();
        Debug.LogWarning("Data Saved successfully");
    }

    private void OnDataLoadSuccess(object response)
    {
        if (response == null) return;
        
        var userDataPayload = (UserDataPayload)response;
        var userDataRaw = userDataPayload.Data;
        var userDataObject = (JObject)userDataRaw;
        
        if (userDataObject == null || userDataObject["user_data"] == null)
        {
            InitEmptyDatabase();
        }

        var loadedPlayerData = JsonConvert.DeserializeObject<PlayerData>(userDataObject["user_data"]?.ToString() ?? string.Empty);

        if (loadedPlayerData == null)
        {
            InitEmptyDatabase();
        }
        else
        {
            _bestScore = loadedPlayerData.BestScore;
            _currentLevel = loadedPlayerData.Level;
            _currentXp = loadedPlayerData.Xp;
            _completedAchievementIds = loadedPlayerData.CompletedAchievementIds ?? new HashSet<string>();

            /*_ownedSkinIds = loadedPlayerData.OwnedSkins ?? new HashSet<string>();
            _ownedSkinIds.Add("Default");
            _currentSkin = loadedPlayerData.CurrentSkin ??  "Default";
            _skinShards = loadedPlayerData.SkinShards;

            if (_currentLevel >= 5) BuySkin("HellFire");
            if (_currentLevel >= 10) BuySkin("AtomicBreak");*/
        }
        LoadingComplete.LoadingCompleteAction?.Invoke();
    }

    private void InitEmptyDatabase()
    {
        _bestScore = 0;
        _currentXp = 0;
        _currentLevel = 0;
        _completedAchievementIds = new HashSet<string>();
        /*_currentSkin = "Default";*/
    }

    private void OnDataSaveError(int code, string message)
    {
        GameManager.OnError?.Invoke(code, message);
    }

    private void OnDataLoadError(int code, string message)
    {
        _bestScore = 0;
        GameManager.OnError?.Invoke(code, message);
    }
}
