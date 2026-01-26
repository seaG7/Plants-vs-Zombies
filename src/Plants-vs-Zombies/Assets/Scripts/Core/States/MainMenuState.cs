using Core.BaseStates;
using Cysharp.Threading.Tasks;
using Data.Path;
using Infrastructure.Services.Scene;
using UnityEngine.SceneManagement;

namespace Core.States
{
    public class MainMenuState : IState, IEnterable, IExitable
    {
        private readonly ISceneLoaderService _sceneLoader;
        private object _sceneInstance;
        private readonly StateMachine _stateMachine;

        public MainMenuState(ISceneLoaderService sceneLoader, StateMachine stateMachine)
        {
            _sceneLoader = sceneLoader;
            _stateMachine = stateMachine;
        }

        public async void Enter()
        {
            _sceneInstance = await _sceneLoader.LoadScene(ScenesPaths.MAIN_MENU, LoadSceneMode.Single);
            
            _sceneInstance = await _sceneLoader.LoadScene(ScenesPaths.GAME, LoadSceneMode.Single);
            
            _stateMachine.ChangeState<GameplayState>();
        }

        public void Exit()
        {
            
        }
    }
}