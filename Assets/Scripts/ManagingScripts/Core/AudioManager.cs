using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    public enum SoundType
    {
        None,
        FoodCollect,
        PreciousFoodCollect,
        RockHit,
        Explode,
        SpeedUp,
        SpeedDown,
        GameOver,
        ButtonSound,
        GameMusic
    }

    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Clips")]
        [SerializeField] private AudioClip gameMusic;
        [SerializeField] private List<SoundClip> clips;

        [System.Serializable]
        public struct SoundClip
        {
            public SoundType Type;
            public AudioClip Clip;
        }

        private Dictionary<SoundType, AudioClip> _clipMap;

        private void Awake()
        {
            _clipMap = new Dictionary<SoundType, AudioClip>();
            foreach (var item in clips)
            {
                if (!_clipMap.ContainsKey(item.Type))
                    _clipMap.Add(item.Type, item.Clip);
            }
        }

        private void OnEnable()
        {
            GameManager.OnGameStarted += PlayMusic;
            GameManager.OnGameOver += PlayGameOverSound;
            Player.OnPlaySound += PlayOneShot; 
        }

        private void OnDisable()
        {
            GameManager.OnGameStarted -= PlayMusic;
            GameManager.OnGameOver -= PlayGameOverSound;
            Player.OnPlaySound -= PlayOneShot;
        }

        private void PlayMusic()
        {
            if (musicSource.clip != gameMusic)
            {
                musicSource.clip = gameMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        private void PlayGameOverSound(int score)
        {
            musicSource.Stop();
            PlayOneShot(SoundType.GameOver);
        }

        public void PlayOneShot(SoundType type)
        {
            if (_clipMap.TryGetValue(type, out var clip))
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }
}