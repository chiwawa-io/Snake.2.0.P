using Luxodd.Game.HelpersAndUtils.Utils;
using System;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public enum NotificationState
    {
        None,
        Notify
    }
    
    public interface INotificationEntity<T>
    {
        T Data { get; }
        IReadOnlyProperty<NotificationState> State { get; }
        void Update();
        void InitPredicate(Predicate<T> notificationChanger, T data);
    }

    public class NotificationEntity<T>: INotificationEntity<T>
    {
        public IReadOnlyProperty<NotificationState> State => _state;

        public T Data => _data;

        private readonly CustomProperty<NotificationState> _state;

        private Predicate<T> _notificationChanger;
        private T _data;

        public NotificationEntity()
        {
            _state = new CustomProperty<NotificationState>(NotificationState.None);
        }
        public void Update()
        {
            var result = _notificationChanger.Invoke(_data);
            _state.SetValue(result ? NotificationState.Notify : NotificationState.None);
        }

        public void InitPredicate(Predicate<T> notificationChanger, T data)
        {
            _notificationChanger = notificationChanger;
            _data = data;
            
            Debug.Log($"[{GetType().Name}][InitPredicate] OK, data: {typeof(T).Name}");
        }
    }
}