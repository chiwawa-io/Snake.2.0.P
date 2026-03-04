using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public enum MissionObjectiveType
    {
        FinishLevel
    }

    public enum MissionState
    {
        Closed,
        InProgress,
        Pause,
        Completed,
        Failed
    }

    public enum MissionRewardState
    {
        NotTaken,
        Taken,
    }
    public enum ProgressValueType
    {
        Counter,
        Timer
    }

    [System.Serializable]
    public class MissionChainData
    {
        [field: SerializeField] public int MinValue { get; private set; }
        [field: SerializeField] public int MaxValue { get; private set; }
        [field: SerializeField] public ProgressValueType ProgressValueType { get; private set; }
    }

    [System.Serializable]
    public class MissionDefinition
    {
        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public MissionObjectiveType Type { get; private set; }
        [field: SerializeField] public string DescriptionKey { get; private set; }
        [field: SerializeField] public int MissionRewardBundleId { get; private set; }
        [field: SerializeField] public List<MissionChainData> ChainData { get; private set; }
        public MissionChainData this[int i] => ChainData[i];
    }

}
