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

        public MainMenuState(ISceneLoaderService sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public void Enter()
        {
            _sceneLoader.LoadScene(ScenesPaths.MAIN_MENU, LoadSceneMode.Single);
        }

        public void Exit()
        {
            
        }
    }
}