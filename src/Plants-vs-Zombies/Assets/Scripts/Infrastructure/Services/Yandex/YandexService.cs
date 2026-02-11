using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YG;

namespace Infrastructure.Services.Yandex
{
    /// <summary>
    /// Facade for YG2 static API. Handles Saves, Ads, and Leaderboards.
    /// </summary>
    public class YandexService : IYandexService
    {
        private Action _interstitialCloseCallback;
        private Action _rewardSuccessCallback;
        private Action _rewardCloseCallback;

        public bool IsSDKEnabled => YG2.isSDKEnabled;

        public YandexService()
        {
            YG2.onCloseInterAdv += OnInterstitialClosed;
            YG2.onRewardAdv += OnRewardSuccess;
        }

        public void GameReady() => YG2.GameReadyAPI();
        public void GameplayStart() => YG2.GameplayStart();
        public void GameplayStop() => YG2.GameplayStop();

        public int GetTotalGold() => YG2.saves.totalGold;

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            
            YG2.saves.totalGold += amount;
            YG2.SaveProgress();
            
            YG2.SetLeaderboard("LeaderboardTotalSuns", YG2.saves.totalGold);
        }

        public float GetMusicVolume() => YG2.saves.musicVolume;
        public float GetSfxVolume() => YG2.saves.sfxVolume;

        public void SetMusicVolume(float value)
        {
            YG2.saves.musicVolume = value;
            YG2.SaveProgress();
        }

        public void SetSfxVolume(float value)
        {
            YG2.saves.sfxVolume = value;
            YG2.SaveProgress();
        }

        public void ShowInterstitial(Action onClosed)
        {
            _interstitialCloseCallback = onClosed;
            YG2.InterstitialAdvShow();
        }

        public void ShowReward(string id, Action onSuccess, Action onClose = null)
        {
            _rewardSuccessCallback = onSuccess;
            _rewardCloseCallback = onClose;
            YG2.RewardedAdvShow(id);
        }

        public string GetText(string key)
        {
            bool isRu = YG2.lang == "ru";
            return key switch
            {
                "RELOADING" => isRu ? "ПЕРЕЗАРЯДКА..." : "RELOADING...",
                "READY" => isRu ? "СНАРЯД ГОТОВ" : "READY TO FIRE",
                "NOT_READY" => isRu ? "Не готово" : "Not Ready",
                "VICTORY" => isRu ? "ПОБЕДА!" : "VICTORY!",
                "GAME_OVER" => isRu ? "ПОРАЖЕНИЕ" : "GAME OVER",
                "TOTAL_SCORE" => isRu ? "Всего очков:" : "Total Score:",
                "START_BATTLE" => isRu ? "В БОЙ!" : "BATTLE!",
                "SETTINGS" => isRu ? "НАСТРОЙКИ" : "SETTINGS",
                "SFX" => isRu ? "Звуки" : "SFX",
                "MUSIC" => isRu ? "Музыка" : "Music",
                _ => key
            };
        }

        private void OnInterstitialClosed()
        {
            _interstitialCloseCallback?.Invoke();
            _interstitialCloseCallback = null;
        }

        private void OnRewardSuccess(string id)
        {
            _rewardSuccessCallback?.Invoke();
            _rewardSuccessCallback = null;
        }
    }
}