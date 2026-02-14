using System;

namespace Services.Backend
{
    public interface IBackendService
    {
        void Initialize(Action onReady, Action onError);
        void StartLevel(Action onSuccess, Action<string> onError);
        void TriggerGameOverFlow(int score, Action onRevive);
    }
}

