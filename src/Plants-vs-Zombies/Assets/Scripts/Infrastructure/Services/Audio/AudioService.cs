using UnityEngine;

namespace Infrastructure.Services.Audio
{
    /// <summary>
    /// Manages global audio settings and volume control.
    /// </summary>
    public class AudioService : IAudioService
    {
        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SfxVolume";

        public float MasterVolume => AudioListener.volume;
        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }

        public AudioService()
        {
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
            SfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
        }

        public void SetMasterVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MUSIC_KEY, MusicVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SFX_KEY, SfxVolume);
            PlayerPrefs.Save();
        }
    }
}