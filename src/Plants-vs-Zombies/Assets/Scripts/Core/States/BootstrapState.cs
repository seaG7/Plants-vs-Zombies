using Core.BaseStates;
using Data.Path;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Window;
using Infrastructure.Services.Yandex;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Core.States
{
    /// <summary>
    /// Initial application entry point. Handles SDK Ready and loading main menu.
    /// </summary>
    public class BootstrapState : IState, IEnterable
    {
        private readonly StateMachine _stateMachine;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly IWindowService _windowService;
        private readonly IYandexService _yandexService;

        public BootstrapState(
            StateMachine stateMachine, 
            ISceneLoaderService sceneLoaderService, 
            IWindowService windowService,
            IYandexService yandexService)
        {
            _stateMachine = stateMachine;
            _sceneLoaderService = sceneLoaderService;
            _windowService = windowService;
            _yandexService = yandexService;
        }

        public async void Enter()
        {
            await _sceneLoaderService.LoadScene(ScenesPaths.MAIN_MENU, LoadSceneMode.Single);
            
            _yandexService.GameReady();
            
            _stateMachine.ChangeState<MainMenuState>();
        }
    }
}