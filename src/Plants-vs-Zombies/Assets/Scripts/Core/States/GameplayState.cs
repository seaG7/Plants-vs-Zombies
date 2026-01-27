using System.Linq;
using Core.BaseStates;
using Core.Interfaces;
using Data.Path;
using Features.Enemy;
using Infrastructure.Providers.Context;
using Infrastructure.Services;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Input;
using Infrastructure.Services.Planting;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Waves;
using Infrastructure.Services.Window;
using UI.HUD;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.States
{
    public class GameplayState : IState, IEnterable, IExitable
    {
        private readonly StateMachine _stateMachine;
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly ILevelProvider _levelProvider;
        private readonly IPlantingService _plantingService;
        private readonly IGridService _gridService;
        private readonly IWaveService _waveService;
        private readonly IPlantTrackerService _plantTracker;
        private readonly ISceneLoaderService _sceneLoader;
        private readonly IEconomyService _economyService;

        private HudWindow _hudWindow;
        private IPossessablePlant _currentPossessedPlant;
        private bool _isInPossessionMode;
        private bool _isBattleStarted;
        private bool _isGameOver;

        private float _nextModeSwitchTime;
        private const float SWITCH_COOLDOWN = 0.7f;

        public GameplayState(
            StateMachine stateMachine,
            IWindowService windowService, 
            IInputService inputService, 
            ICameraService cameraService,
            ILevelProvider levelProvider,
            IPlantingService plantingService,
            IGridService gridService,
            IWaveService waveService,
            IPlantTrackerService plantTracker,
            ISceneLoaderService sceneLoader,
            IEconomyService economyService)
        {
            _stateMachine = stateMachine;
            _windowService = windowService;
            _inputService = inputService;
            _cameraService = cameraService;
            _levelProvider = levelProvider;
            _plantingService = plantingService;
            _gridService = gridService;
            _waveService = waveService;
            _plantTracker = plantTracker;
            _sceneLoader = sceneLoader;
            _economyService = economyService;
        }

        public async void Enter()
        {
            _isGameOver = false;
            _isBattleStarted = false; 
            
            _hudWindow = await _windowService.OpenAndGet<HudWindow>(WindowID.HUD);
            _hudWindow.BindButtons(OnRestartClicked, OnMenuClicked, StartBattle);
            _hudWindow.SetStartButtonVisible(true);
            
            _inputService.Enable();
            _inputService.OnCancelPerformed += HandleEsc;
            _inputService.OnClickPerformed += HandleClick;
            _inputService.OnHotbarHotkeyPressed += HandleHotbarInput;
            
            _plantingService.Initialize();
            
            if (_levelProvider.CurrentLevel != null)
                SubscribeToLevel();
            else
                _levelProvider.OnLevelLoaded += SubscribeToLevel;

            EnterPlantingMode();
        }

        public void Exit()
        {
            _windowService.Close(WindowID.HUD);
            _inputService.Disable();
            _inputService.OnCancelPerformed -= HandleEsc;
            _inputService.OnClickPerformed -= HandleClick;
            _inputService.OnHotbarHotkeyPressed -= HandleHotbarInput;
            
            if (_levelProvider.CurrentLevel?.FinishTrigger != null)
                _levelProvider.CurrentLevel.FinishTrigger.OnZombieCrossed -= HandleDefeat;
            
            _levelProvider.OnLevelLoaded -= SubscribeToLevel;
            
            _plantingService.Dispose();
        }

        private void SubscribeToLevel()
        {
             _levelProvider.OnLevelLoaded -= SubscribeToLevel;
            if (_levelProvider.CurrentLevel?.FinishTrigger != null)
            {
                _levelProvider.CurrentLevel.FinishTrigger.OnZombieCrossed -= HandleDefeat;
                _levelProvider.CurrentLevel.FinishTrigger.OnZombieCrossed += HandleDefeat;
            }
        }

        private void StartBattle()
        {
            if (_isBattleStarted) return;
            
            _isBattleStarted = true;
            _hudWindow.SetStartButtonVisible(false);
            _waveService.StartLevel();

            var activePlants = _plantTracker.GetAll();
            if (activePlants.Count > 0)
            {
                EnterPossessionMode(activePlants[0]);
            }
        }

        private void HandleDefeat(ZombieController killer)
        {
            if (_isGameOver) return;
            _isGameOver = true;
            _waveService.StopLevel();

            killer.StopMovement();

            var enemies = _levelProvider.CurrentLevel.ActiveEnemies.ToList();
            foreach (var enemy in enemies)
            {
                if (enemy is ZombieController z && z != killer)
                    Object.Destroy(z.gameObject);
            }

            if (_currentPossessedPlant != null)
            {
                _currentPossessedPlant.SetPossessed(false);
                _currentPossessedPlant = null;
            }
            _isInPossessionMode = false;
            
            _cameraService.SetTacticalView(_levelProvider.CurrentLevel.CameraTacticalPoint);
            
            _hudWindow.SetGameplayVisibility(false);
            _hudWindow.SetActivePlant(null);
            _hudWindow.SetStartButtonVisible(false);

            _inputService.Disable();
            _plantingService.ClearSelection();
            
            _hudWindow.ShowGameOverPanel();
        }

        private void OnRestartClicked()
        {
            _stateMachine.ChangeState<GameLoadState>();
        }

        private void OnMenuClicked()
        {
            _stateMachine.ChangeState<MainMenuState>();
        }

        private void HandleHotbarInput(int keyIndex)
        {
            if (_isGameOver || Time.time < _nextModeSwitchTime) return;
            
            int listIndex = keyIndex - 1;
            var plant = _plantTracker.GetPlantByIndex(listIndex);
            
            if (plant != null && _currentPossessedPlant != plant)
            {
                EnterPossessionMode(plant);
            }
        }

        private void HandleClick()
        {
            if (_isGameOver || Time.time < _nextModeSwitchTime) return;

            if (_isInPossessionMode) return;
            
            Vector2 mousePos = _inputService.GetPointerPosition();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                if (_gridService.WorldToGrid(hit.point, out int lane, out int row))
                {
                    if (_gridService.IsCellOccupied(lane, row))
                    {
                        var plantObj = _gridService.GetPlantAt(lane, row);
                        var plant = plantObj.GetComponent<IPossessablePlant>();
                        if (plant != null) EnterPossessionMode(plant);
                    }
                    else
                    {
                        _plantingService.TryPlantAtCursor();
                    }
                }
            }
        }

        private void HandleEsc()
        {
            if (_isGameOver || Time.time < _nextModeSwitchTime) return;
            EnterPlantingMode();
        }

        private void EnterPlantingMode()
        {
            if (!_isInPossessionMode && _currentPossessedPlant == null)
            {
                _plantingService.ClearSelection();
            }
            else
            {
                _nextModeSwitchTime = Time.time + SWITCH_COOLDOWN;
            }
            
            _hudWindow.SetActivePlant(null);

            if (_currentPossessedPlant != null)
            {
                _currentPossessedPlant.SetPossessed(false);
                _currentPossessedPlant = null;
            }

            _isInPossessionMode = false;
            _plantingService.ClearSelection(); 

            _cameraService.SetTacticalView(_levelProvider.CurrentLevel.CameraTacticalPoint);
            _hudWindow.SetGameplayVisibility(true);
        }

        private async void EnterPossessionMode(IPossessablePlant plant)
        {
            if (!_isBattleStarted) StartBattle();

            _nextModeSwitchTime = Time.time + SWITCH_COOLDOWN;
            
            if (_currentPossessedPlant != null) _currentPossessedPlant.SetPossessed(false);
            
            _isInPossessionMode = true;
            _currentPossessedPlant = plant;
            
            _plantingService.ClearSelection();
            
            _hudWindow.SetGameplayVisibility(false);
            _hudWindow.SetActivePlant(plant);
            
            await _cameraService.MoveToTarget(plant.CameraMountPoint);
            plant.SetPossessed(true);
        }
    }
}