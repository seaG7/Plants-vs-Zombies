using Core.BaseStates;
using Data.Path;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Window;
using Infrastructure.Services.Yandex;
using UnityEngine.SceneManagement;

namespace Core.States
{
    /// <summary>
    /// Main menu state. Marks GameplayStart as UI is interactive.
    /// </summary>
    public class MainMenuState : IState, IEnterable, IExitable
    {
        private readonly ISceneLoaderService _sceneLoader;
        private readonly IWindowService _windowService;
        private readonly IAudioService _audioService;
        private readonly IStaticDataProvider _staticData;
        private readonly IYandexService _yandexService;

        public MainMenuState(
            ISceneLoaderService sceneLoader, 
            IWindowService windowService, 
            IAudioService audioService,
            IStaticDataProvider staticData,
            IYandexService yandexService)
        {
            _sceneLoader = sceneLoader;
            _windowService = windowService;
            _audioService = audioService;
            _staticData = staticData;
            _yandexService = yandexService;
        }

        public async void Enter()
        {
            _yandexService.GameplayStart();

            await _windowService.Open(WindowID.MainMenu);

            var gameConfig = _staticData.GetGameConfig();
            if (gameConfig != null && gameConfig.mainMenuMusic != null)
            {
                _audioService.InitializeMusicSource();
                _audioService.PlayMusic(gameConfig.mainMenuMusic);
            }
            
            _windowService.Close(WindowID.Loading);
        }

        public void Exit()
        {
            _windowService.Close(WindowID.MainMenu);
            if (_windowService.IsWindowOpened(WindowID.Settings))
                _windowService.Close(WindowID.Settings);
        }
    }
}