using Core.BaseStates;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Scene;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Core.States
{
    public class BootstrapState : IState, IEnterable
    {
        private readonly StateMachine _stateMachine;
        private readonly ISceneLoaderService _sceneLoaderService;

        public BootstrapState(StateMachine stateMachine, ISceneLoaderService sceneLoaderService)
        {
            _stateMachine = stateMachine;
            _sceneLoaderService = sceneLoaderService;
        }

        public async void Enter()
        {
            _stateMachine.ChangeState<MainMenuState>();
        }
    }
}