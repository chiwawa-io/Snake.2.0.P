using System;
using UnityEngine;
using System.Collections.Generic;
using Core.Events;
using Zenject;

namespace Services.Audio
{
    public enum SoundType
    {
        None,
        FoodCollect,
        PreciousFoodCollect,
        SpeedUp,
        SpeedDown,
        GameOver,
        ButtonSound
    }

    public class AudioManager : MonoBehaviour, IAudioService
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;

        [Header("Clips")]
        [SerializeField] private AudioClip _gameMusic;
        [SerializeField] private List<SoundClip> _clips;

        [Serializable]
        public struct SoundClip
        {
            public SoundType Type;
            public AudioClip Clip;
        }

        private Dictionary<SoundType, AudioClip> _clipMap;
        private SignalBus _signalBus;
        
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        private void Awake()
        {
            _clipMap = new Dictionary<SoundType, AudioClip>();
            foreach (var item in _clips)
            {
                if (!_clipMap.ContainsKey(item.Type))
                    _clipMap.Add(item.Type, item.Clip);
            }
        }

        private void Start()
        {
            if (_signalBus == null) return;
            _signalBus.Subscribe<PlaySoundSignal>(OnPlaySound);
            _signalBus.Subscribe<GameStartedSignal>(PlayMusic);
            _signalBus.Subscribe<GameOverSignal>(PlayGameOverSound);
        }

        private void OnDisable()
        {
            if (_signalBus == null) return;
            _signalBus.Unsubscribe<PlaySoundSignal>(OnPlaySound);
            _signalBus.Unsubscribe<GameStartedSignal>(PlayMusic);
            _signalBus.Unsubscribe<GameOverSignal>(PlayGameOverSound);
        }

        public void PlayOneShot(SoundType type)
        {
            if (_clipMap.TryGetValue(type, out var clip))
            {
                _sfxSource.PlayOneShot(clip);
            }
        }

        private void PlayMusic()
        {
            if (_musicSource.clip != _gameMusic)
            {
                _musicSource.clip = _gameMusic;
                _musicSource.loop = true;
                _musicSource.Play();
            }
        }

        private void OnPlaySound(PlaySoundSignal signal)
        {
            PlayOneShot(signal.Type);
        }

        private void PlayGameOverSound()
        {
            _musicSource.Stop();
            PlayOneShot(SoundType.GameOver);
        }
    }
}