using System;
using UnityEngine;

namespace Infrastructure.Services.Audio
{
    public interface IAudioService
    {
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SfxVolume { get; }

        void SetMasterVolume(float value);
        void SetMusicVolume(float value);
        void SetSfxVolume(float value);
        void PlayMusic(AudioClip clip);
    }
}