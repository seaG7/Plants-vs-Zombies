using Core.BaseStates;
using Data.Path;
using Infrastructure.Services;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Window;
using UnityEngine.SceneManagement;

namespace Core.States
{
    /// <summary>
    /// Initial application entry point. Shows loading screen and loads the main menu.
    /// </summary>
    public class BootstrapState : IState, IEnterable
    {
        private readonly StateMachine _stateMachine;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly IWindowService _windowService;

        public BootstrapState(StateMachine stateMachine, ISceneLoaderService sceneLoaderService, IWindowService windowService)
        {
            _stateMachine = stateMachine;
            _sceneLoaderService = sceneLoaderService;
            _windowService = windowService;
        }

        public async void Enter()
        {
            await _windowService.Open(WindowID.Loading);
            await _sceneLoaderService.LoadScene(ScenesPaths.MAIN_MENU, LoadSceneMode.Single);
            _stateMachine.ChangeState<MainMenuState>();
        }
    }
}