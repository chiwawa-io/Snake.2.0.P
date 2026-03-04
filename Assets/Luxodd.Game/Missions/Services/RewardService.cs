using System;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public class RewardService : MonoBehaviour
    {
        [SerializeField] private RewardDataBase _rewardDataBase;
        [SerializeField] private ItemResourceProvider _itemResourceProvider;
        [SerializeField] private ItemsDataBase _itemsDataBase;
        

        private Action _onComplete;
        
        public void ApplyReward(int rewardId, Action onComplete)
        {
            _onComplete = onComplete;
            
            //get a reward bundle from the reward database
            PrepareRewardPopup(rewardId);
            
            //prepare popup with necessary items, and amount
            //show popup
            //setup ok button click callback
            //invoke action after okay button
        }

        private void PrepareRewardPopup(int rewardId)
        {
            var rewardBundle = _rewardDataBase.GetReward(rewardId);
            foreach (var itemData in rewardBundle.Rewards)
            {
                var itemDataDescriptor = _itemsDataBase[itemData.Type];
                var itemSprite = _itemResourceProvider.ProvideSprite(itemDataDescriptor.SpriteKey);
                //TODO: prepare a visual display of awards
            }
        }

        private void OnOkButtonClicked()
        {
            _onComplete?.Invoke();
        }
        
        //Testing
        [ContextMenu("Test Add Reward")]
        private void TestAddReward()
        {
            ApplyReward(0, OnRewardDisplayAction);
        }

        private void OnRewardDisplayAction()
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnRewardDisplayAction)}] OK");
        }
    }
}
