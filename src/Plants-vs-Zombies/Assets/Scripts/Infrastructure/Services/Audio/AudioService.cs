using Infrastructure.Services.Yandex;
using UnityEngine;

namespace Infrastructure.Services.Audio
{
    /// <summary>
    /// Manages audio volumes using Yandex saves for persistence.
    /// </summary>
    public class AudioService : IAudioService
    {
        private readonly IYandexService _yandexService;
        private AudioSource _musicSource;
        private GameObject _musicObject;

        public float MasterVolume => AudioListener.volume;
        public float MusicVolume => _yandexService.GetMusicVolume();
        public float SfxVolume => _yandexService.GetSfxVolume();

        public AudioService(IYandexService yandexService)
        {
            _yandexService = yandexService;
        }

        public void InitializeMusicSource()
        {
            if (_musicObject == null)
            {
                _musicObject = new GameObject("MusicSource_Global");
                Object.DontDestroyOnLoad(_musicObject);
                _musicSource = _musicObject.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.volume = MusicVolume;
            }
        }

        public void SetMasterVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
        }

        public void SetMusicVolume(float value)
        {
            float clamped = Mathf.Clamp01(value);
            _yandexService.SetMusicVolume(clamped);
            
            if (_musicSource != null)
                _musicSource.volume = clamped;
        }

        public void SetSfxVolume(float value)
        {
            float clamped = Mathf.Clamp01(value);
            _yandexService.SetSfxVolume(clamped);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (_musicSource == null) InitializeMusicSource();

            if (_musicSource.clip == clip && _musicSource.isPlaying) return;

            _musicSource.clip = clip;
            _musicSource.volume = MusicVolume;
            _musicSource.Play();
        }
    }
}