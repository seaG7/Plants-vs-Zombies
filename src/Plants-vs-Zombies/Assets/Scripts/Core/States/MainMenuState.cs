using Core.BaseStates;
using Data.Path;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Window;
using UnityEngine.SceneManagement;

namespace Core.States
{
    /// <summary>
    /// Handles main menu initialization and hides the loading screen.
    /// </summary>
    public class MainMenuState : IState, IEnterable, IExitable
    {
        private readonly ISceneLoaderService _sceneLoader;
        private readonly IWindowService _windowService;
        private readonly IAudioService _audioService;
        private readonly IStaticDataProvider _staticData;

        public MainMenuState(
            ISceneLoaderService sceneLoader, 
            IWindowService windowService, 
            IAudioService audioService,
            IStaticDataProvider staticData)
        {
            _sceneLoader = sceneLoader;
            _windowService = windowService;
            _audioService = audioService;
            _staticData = staticData;
        }

        public async void Enter()
        {
            if (SceneManager.GetActiveScene().name != ScenesPaths.MAIN_MENU)
            {
                await _sceneLoader.LoadScene(ScenesPaths.MAIN_MENU, LoadSceneMode.Single);
            }

            await _windowService.Open(WindowID.MainMenu);

            var gameConfig = _staticData.GetGameConfig();
            if (gameConfig != null && gameConfig.mainMenuMusic != null)
            {
                _audioService.PlayMusic(gameConfig.mainMenuMusic);
            }
            
            _windowService.Close(WindowID.Loading);
        }

        public async void Exit()
        {
            _windowService.Close(WindowID.MainMenu);
            if (_windowService.IsWindowOpened(WindowID.Settings))
            {
                _windowService.Close(WindowID.Settings);
            }
        }
    }
}