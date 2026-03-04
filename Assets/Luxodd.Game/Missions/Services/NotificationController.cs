using Luxodd.Game.HelpersAndUtils.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public interface INotificationController<T>
    {
        IBoolReadOnlyProperty Notified { get; }
        IIntReadOnlyProperty NotificationsCount { get; }

        void AddNotification(Predicate<T> notificationChanger, T data, out System.Action valueChangeCallback);
        void UpdateState();
        List<INotificationEntity<T>> GetNotificationEntities();
    }

    public class NotificationController : MonoBehaviour, INotificationController<IMissionChainInfo>
    {
        public IBoolReadOnlyProperty Notified => _notifiedProperty;
        public IIntReadOnlyProperty NotificationsCount => _notificationsCount;

        private readonly BoolProperty _notifiedProperty = new BoolProperty();
        private readonly IntProperty _notificationsCount = new IntProperty();

        private List<INotificationEntity<IMissionChainInfo>> _notificationEntities = new List<INotificationEntity<IMissionChainInfo>>();

        public void AddNotification(Predicate<IMissionChainInfo> notificationChanger, IMissionChainInfo data, out Action valueChangeCallback)
        {
            var notificationEntity = CreateNotificationEntity(notificationChanger, data);
            notificationEntity.State.AddListener(OnNotificationStateChanged);
            _notificationEntities.Add(notificationEntity);
            valueChangeCallback = notificationEntity.Update;
        }

        public void UpdateState()
        {
            UpdateNotifications();
        }

        public List<INotificationEntity<IMissionChainInfo>> GetNotificationEntities()
        {
            return _notificationEntities;
        }

        private INotificationEntity<IMissionChainInfo> CreateNotificationEntity(Predicate<IMissionChainInfo> notificationChanger, IMissionChainInfo data)
        {
            var notificationEntity = new NotificationEntity<IMissionChainInfo>();
            notificationEntity.InitPredicate(notificationChanger, data);
            return notificationEntity;
        }

        private void OnNotificationStateChanged(NotificationState state)
        {
            UpdateNotifications();
        }

        private void UpdateNotifications()
        {
            var notificationCount = 0;
            foreach (var notificationEntity in _notificationEntities)
            {
                if (notificationEntity.State.Value == NotificationState.Notify)
                {
                    notificationCount++;
                }
            }

            _notificationsCount.SetValue(notificationCount);
            _notifiedProperty.SetValue(notificationCount > 0);

            Debug.Log($"[{GetType().Name}][OnNotificationStateChanged] OK, data: IMissionChainInfo, notifications Count: {notificationCount}");
        }
    }

}