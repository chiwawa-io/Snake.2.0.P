using System;
using System.Collections.Generic;
using System.Linq;
using Game.UI.Contexts;
using Game.UI.Items.Missions;
using Luxodd.Game.Scripts.Game;
using Luxodd.Game.Scripts.HelpersAndUtils;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using Luxodd.Game.Scripts.Network.CommandHandler;
using Luxodd.Game.Scripts.Network.Payloads;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions.Testing
{
    public class MissionExampleStartBehaviour : MonoBehaviour
    {
        private const string Pay2Play = "p2p";
        private const string StrategicBetting = "sb";

        [Header("UI References")]
        [SerializeField] private Transform _previewParent;
        [SerializeField] private Transform _resultParent;
        [SerializeField] private MissionViewContext _missionViewContext;
        [SerializeField] private MissionPreviewWindowHandler _previewWindow;
        [SerializeField] private MissionResultWindowHandler _resultWindow;
        [SerializeField] private MissionTypeItemView _missionTypePrefab;
        [SerializeField] private MissionPreviewItemView _previewItemPrefab;
        [SerializeField] private MissionResultItemView _resultItemPrefab;

        [Header("Services")]
        [SerializeField] private WebSocketCommandHandler _commandHandler;
        [SerializeField] private MissionService _missionService;

        [Header("Preview Colors")]
        [SerializeField] private Color _colorA = Color.blue;
        [SerializeField] private Color _colorB = Color.cyan;

        private StrategicBettingData _strategicData;
        private Action _onRequestCompleted;
        private GameType _testGameType;

        #region Unity

        private void Start()
        {
            _resultWindow.HidePanel();
            SubscribeToUI();
        }

        [ContextMenu("Test Prepare Mission Preview")]
        public void TestMissions()
        {
            RequestSessionInfo(PreparePreview);

        }

        #endregion

        #region Session Flow

        public void RequestSessionInfo(Action onCompleted)
        {
            _onRequestCompleted = onCompleted;

            _commandHandler.SendGetGameSessionInfoRequestCommand(
                OnSessionSuccess,
                OnSessionError);

            CoroutineManager.DelayedAction(0.5f, () => { TestGameSessionInfoResponse(); });
        }

        private void OnSessionSuccess(SessionInfoPayload payload)
        {
            LoggerHelper.Log($"[{GetType().Name}] Session received");

            ProcessSessionPayload(payload);

            _onRequestCompleted?.Invoke();
            ClearCallback();
        }

        private void OnSessionError(int code, string message)
        {
            LoggerHelper.LogError($"Session error: {code}, {message}");
            _onRequestCompleted?.Invoke();
            ClearCallback();
        }

        private void ProcessSessionPayload(SessionInfoPayload payload)
        {
            var token = JToken.FromObject(payload.Data);
            var sessionInfo = token.ToObject<GameSessionInfoData>();

            if (!Enum.TryParse(sessionInfo.LevelDifficulty, true, out DifficultyLevel difficulty))
                difficulty = DifficultyLevel.Easy;

            var missions = token["missions"]?.ToObject<List<MissionBettingInfo>>()
                           ?? new List<MissionBettingInfo>();

            _strategicData = new StrategicBettingData
            {
                LevelId = sessionInfo.LevelId,
                LevelDifficulty = difficulty,
                Missions = missions
            };

           // _previewWindow.ShowPanel();
            _missionService.PrepareSelectedMissionList(_strategicData);
        }

        private void ClearCallback()
        {
            //_onRequestCompleted = null;
        }

        #endregion

        #region Preview
        public void PreparePreview()
        {
            if (_strategicData == null)
            {
                Debug.LogError("Strategic data is null");
                return;
            }

            _previewWindow.ShowPanel();
            ClearChildren(_previewParent);

            var missionMap = _missionViewContext.MissionDataBase.Missions
                .ToDictionary(x => x.Id);

            MissionType currentType = default;
            int index = 0;

            foreach (var betting in _strategicData.Missions)
            {
                if (!missionMap.TryGetValue(betting.MissionId, out var missionData))
                    continue;

                if (missionData.Type != currentType)
                {
                    currentType = missionData.Type;
                    CreateMissionHeader(currentType, _previewParent);
                    index = 0;
                }

                CreatePreviewItem(missionData, betting, index++);
            }
        }

        private void CreatePreviewItem(MissionData data, MissionBettingInfo betting, int index)
        {
            var view = Instantiate(_previewItemPrefab, _previewParent);

            float ratio = betting.Ratio <= 0 ? 2f : betting.Ratio;
            float payout = betting.Bet * ratio;

            view.SetMissionData(data.Description, betting.Bet, ratio, payout);
            view.SetBackgroundColor(index % 2 == 0 ? _colorA : _colorB);
            view.SetToggleCallBack(() =>
                _missionService.CompleteMissionById(betting.MissionId));
        }

        #endregion

        #region Results

        public void ShowResults()
        {
            ClearChildren(_resultParent);

            var results = BuildResults();
            PopulateResults(results);
        }

        private List<MissionUIResult> BuildResults()
        {
            var result = new List<MissionUIResult>();
            var missionMap = _missionViewContext.MissionDataBase.Missions
                .ToDictionary(x => x.Id);

            foreach (var betting in _strategicData.Missions)
            {
                if (!missionMap.TryGetValue(betting.MissionId, out var missionData))
                    continue;

                bool isWin = IsMissionCompleted(missionData.Id);

                result.Add(new MissionUIResult
                {
                    MissionData = missionData,
                    BettingInfo = betting,
                    Payout = betting.Bet * betting.Ratio,
                    IsWin = isWin
                });
            }

            return result;
        }

        private void PopulateResults(List<MissionUIResult> results)
        {
            MissionType currentType = default;
            int index = 0;

            foreach (var r in results)
            {
                if (r.MissionData.Type != currentType)
                {
                    currentType = r.MissionData.Type;
                    CreateMissionHeader(currentType, _resultParent);
                    index = 0;
                }

                var view = Instantiate(_resultItemPrefab, _resultParent);

                view.SetMissionData(
                    r.MissionData.Name,
                    r.BettingInfo.Bet,
                    r.BettingInfo.Ratio,
                    r.Payout,
                    r.IsWin);

                view.SetBackgroundColor(index++ % 2 == 0 ? _colorA : _colorB);
            }
        }

        private bool IsMissionCompleted(string missionId)
        {
            var states = _missionService.GetMissionStatesByMissionId(missionId);
            return states.Count > 0 &&
                   states.All(x => x == MissionState.Completed);
        }

        #endregion

        #region UI Helpers

        private void CreateMissionHeader(MissionType type, Transform parent)
        {
            var header = Instantiate(_missionTypePrefab, parent);
            header.SetMissionType(type);
        }

        private void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        private void SubscribeToUI()
        {
            _previewWindow.SetNextButtonClickedCallback(OnPreviewNext);
            _resultWindow.SetNextButtonClickedCallback(() =>
                _resultWindow.HidePanel());
        }

        private void OnPreviewNext()
        {
            _previewWindow.HidePanel();
            _resultWindow.ShowPanel();
            ShowResults();
        }

        #endregion


        #region Local Tests
        private void TestGameSessionInfoResponse()
        {
            var testCallback = _testGameType == GameType.StrategicBetting // delete after
                ? TestGetGameSessionInfoResponseP2P()
                : TestGetGameSessionInfoResponseStrategicBetting();

            OnSessionSuccess(testCallback);
        }

        private SessionInfoPayload TestGetGameSessionInfoResponseP2P()
        {
            return new SessionInfoPayload()
            {
                SessionType = Pay2Play,
                Data = null
            };
        }

        private SessionInfoPayload TestGetGameSessionInfoResponseStrategicBetting()
        {
            var strategicBettingData = PrepareStrategicBettingDataForTest();

            return new SessionInfoPayload()
            {
                SessionType = StrategicBetting,
                Data = strategicBettingData
            };
        }

        private StrategicBettingData PrepareStrategicBettingDataForTest()
        {
            var missionBettingInfoList = new List<MissionBettingInfo>()
            {
                new MissionBettingInfo()
                {
                    MissionId = "mission_main_1",
                    Bet = 3f,
                    CalculatedHardness = 1
                },
                new MissionBettingInfo()
                {
                    MissionId = "mission_independent_1",
                    Bet = 6f,
                    CalculatedHardness = 1
                },
                new MissionBettingInfo()
                {
                    MissionId = "mission_dependent_2",
                    Bet = 4.5f,
                    CalculatedHardness = 1
                }
            };

            return new StrategicBettingData()
            {
                LevelId = 1,
                Missions = missionBettingInfoList,
                LevelDifficulty = DifficultyLevel.Easy
            };
        }

        #endregion
    }

    public class MissionUIResult
    {
        public MissionData MissionData;
        public MissionBettingInfo BettingInfo;
        public float Payout;
        public bool IsWin;
    }
}
