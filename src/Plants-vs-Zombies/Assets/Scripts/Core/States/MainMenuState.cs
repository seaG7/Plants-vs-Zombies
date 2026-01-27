using Core.BaseStates;
using Data.Path;
using Infrastructure.Services;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Window;
using UnityEngine.SceneManagement;

namespace Core.States
{
    /// <summary>
    /// Handles the main menu scene logic and UI initialization.
    /// </summary>
    public class MainMenuState : IState, IEnterable, IExitable
    {
        private readonly ISceneLoaderService _sceneLoader;
        private readonly IWindowService _windowService;

        public MainMenuState(ISceneLoaderService sceneLoader, IWindowService windowService)
        {
            _sceneLoader = sceneLoader;
            _windowService = windowService;
        }

        public async void Enter()
        {
            await _sceneLoader.LoadScene(ScenesPaths.MAIN_MENU, LoadSceneMode.Single);
            await _windowService.Open(WindowID.MainMenu);
        }

        public void Exit()
        {
            _windowService.Close(WindowID.MainMenu);
            if (_windowService.IsWindowOpened(WindowID.Settings))
            {
                _windowService.Close(WindowID.Settings);
            }
        }
    }
}