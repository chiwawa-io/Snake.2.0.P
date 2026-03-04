using System;
using System.Collections.Generic;
using Luxodd.Game.Scripts.Missions;
using Luxodd.Game.Scripts.Network.Payloads;
using Services.Gameloop;

namespace Services.Backend
{
    public interface IBackendService
    {
        void Initialize(Action onReady, Action onError);
        void StartLevel(Action onSuccess, Action<string> onError);
        void TriggerGameOverFlow(int score, int finalLength, GameSessionStats stats, StrategicBettingData sbData, Action onRevive);
        
        void GetGameSessionInfo(Action<SessionInfoPayload, StrategicBettingData> onSuccess, Action<int, string> onError);
        void GetSeedForRNG(int seed);
        // void SendStrategicBettingResult(List<MissionResultDto> results, Action onSuccess, Action<string> onError);
    }
}

