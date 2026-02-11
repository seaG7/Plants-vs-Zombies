using System;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.Yandex
{
    /// <summary>
    /// Contract for Yandex Games SDK features: Ads, Leaderboards, Saves, Localization.
    /// </summary>
    public interface IYandexService
    {
        bool IsSDKEnabled { get; }
        
        // Progression
        int GetTotalGold();
        void AddGold(int amount);
        
        // Audio Settings
        float GetMusicVolume();
        float GetSfxVolume();
        void SetMusicVolume(float value);
        void SetSfxVolume(float value);
        
        // Ads
        void ShowInterstitial(Action onClosed);
        void ShowReward(string id, Action onSuccess, Action onClose = null);
        
        // Game Flow
        void GameReady();
        void GameplayStart();
        void GameplayStop();
        
        // Localization
        string GetText(string key);
    }
}