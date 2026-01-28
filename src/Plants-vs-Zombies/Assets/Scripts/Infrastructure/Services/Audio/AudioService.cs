using UnityEngine;

namespace Infrastructure.Services.Audio
{
    /// <summary>
    /// Manages global audio, volume control, and background music persistence.
    /// </summary>
    public class AudioService : IAudioService
    {
        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SfxVolume";

        private AudioSource _musicSource;
        private GameObject _musicObject;

        public float MasterVolume => AudioListener.volume;
        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }

        public AudioService()
        {
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
            SfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
            
            InitializeMusicSource();
        }

        private void InitializeMusicSource()
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
            MusicVolume = Mathf.Clamp01(value);
            if (_musicSource != null)
            {
                _musicSource.volume = MusicVolume;
            }
            PlayerPrefs.SetFloat(MUSIC_KEY, MusicVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SFX_KEY, SfxVolume);
            PlayerPrefs.Save();
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || (_musicSource.clip == clip && _musicSource.isPlaying)) return;

            _musicSource.clip = clip;
            _musicSource.volume = MusicVolume;
            _musicSource.Play();
        }
    }
}