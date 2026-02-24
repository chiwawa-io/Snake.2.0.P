using System;
using System.Collections.Generic;
using Luxodd.Game.Scripts.Missions;
using Luxodd.Game.Scripts.Network.Payloads;

namespace Services.Backend
{
    public interface IBackendService
    {
        void Initialize(Action onReady, Action onError);
        void StartLevel(Action onSuccess, Action<string> onError);
        void TriggerGameOverFlow(int score, Action onRevive);
        
        // public void GetGameSessionInfo(Action<BettingSessionMissionsPayload> onSuccess, Action<int, string> onError);
        // void SendStrategicBettingResult(List<MissionResultDto> results, Action onSuccess, Action<string> onError);
    }
}

