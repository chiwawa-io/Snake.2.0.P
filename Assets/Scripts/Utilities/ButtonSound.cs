using Services.Audio;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Utilities 
{
    public class ButtonSoundLinker : MonoBehaviour
    {
        private AudioManager _audioManager;

        [Inject]
        public void Construct(AudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        private void Start()
        {
            Button button = GetComponent<Button>();
            button.onClick.AddListener(() => _audioManager.PlayOneShot(SoundType.ButtonSound));
        }
    }
}