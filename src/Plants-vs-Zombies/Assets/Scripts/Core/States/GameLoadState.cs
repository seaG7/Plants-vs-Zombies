using Cysharp.Threading.Tasks;
using Core.BaseStates; 
using Data.Path;
using Infrastructure.Factories.Objects;
using Infrastructure.Services;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Planting;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Waves;
using Infrastructure.Services.Window;
using UnityEngine.SceneManagement;

namespace Core.States
{
    public class GameLoadState : IState, IEnterable
    {
        private readonly StateMachine _stateMachine;
        private readonly ISceneLoaderService _sceneLoader;
        private readonly IWindowService _windowService;
        private readonly IEconomyService _economyService;
        private readonly IGridService _gridService;
        private readonly IPlantTrackerService _plantTracker;
        private readonly IWaveService _waveService;
        private readonly IGameObjectFactory _factory;

        public GameLoadState(
            StateMachine stateMachine,
            ISceneLoaderService sceneLoader,
            IWindowService windowService,
            IEconomyService economyService,
            IGridService gridService,
            IPlantTrackerService plantTracker,
            IWaveService waveService,
            IGameObjectFactory factory)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _windowService = windowService;
            _economyService = economyService;
            _gridService = gridService;
            _plantTracker = plantTracker;
            _waveService = waveService;
            _factory =  factory;
        }

        public async void Enter()
        {
            // Открываем окно
            await _windowService.Open(WindowID.Loading);
            
            // Ждем 1 кадр, чтобы окно точно появилось перед фризом загрузки сцены
            await UniTask.DelayFrame(1);

            _economyService.Reset();
            _gridService.Reset();
            _plantTracker.Clear();
            _waveService.StopLevel();
            _factory.Cleanup();

            await _sceneLoader.LoadScene(ScenesPaths.GAME, LoadSceneMode.Single);

            _stateMachine.ChangeState<GameplayState>();
        }
    }
}