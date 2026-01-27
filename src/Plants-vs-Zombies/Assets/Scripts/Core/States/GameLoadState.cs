using Core.BaseStates; 
using Data.Path;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Planting;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Waves;
using UnityEngine.SceneManagement;

namespace Core.States
{
    public class GameLoadState : IState, IEnterable
    {
        private readonly StateMachine _stateMachine;
        private readonly ISceneLoaderService _sceneLoader;

        private readonly IEconomyService _economyService;
        private readonly IGridService _gridService;
        private readonly IPlantTrackerService _plantTracker;
        private readonly IWaveService _waveService;
        private readonly IGameObjectFactory _factory;

        public GameLoadState(
            StateMachine stateMachine,
            ISceneLoaderService sceneLoader,
            IEconomyService economyService,
            IGridService gridService,
            IPlantTrackerService plantTracker,
            IWaveService waveService,
            IGameObjectFactory factory)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _economyService = economyService;
            _gridService = gridService;
            _plantTracker = plantTracker;
            _waveService = waveService;
            _factory =  factory;
        }

        public async void Enter()
        {
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