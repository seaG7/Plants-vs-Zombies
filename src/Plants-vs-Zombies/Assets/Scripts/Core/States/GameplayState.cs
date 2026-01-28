using System.Linq;
using Cysharp.Threading.Tasks; // Для UniTask если понадобится
using Core.BaseStates;
using Core.Interfaces;
using Data.Enums;
using Data.Path;
using Features.Enemy;
using Features.Plants;
using Infrastructure.Providers.Context;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services;
using Infrastructure.Services.Audio;
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
        private readonly IStaticDataProvider _staticData;
        private readonly IAudioService _audioService;

        private HudWindow _hudWindow;
        private IPossessablePlant _currentPossessedPlant;
        private bool _isInPossessionMode;
        private bool _isBattleStarted;
        private bool _isGameOver;

        private float _nextModeSwitchTime;
        private const float SWITCH_COOLDOWN = 0.7f;
        
        // --- Tutorial State ---
        private bool _isTutorialComplete = false;
        private int _tutorialStep = 0; 

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
            IEconomyService economyService,
            IStaticDataProvider staticData,
            IAudioService audioService)
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
            _staticData = staticData;
            _audioService = audioService;
        }

        public async void Enter()
        {
            _isGameOver = false;
            _isBattleStarted = false; 
            _isTutorialComplete = false;
            _tutorialStep = 0;
            
            _hudWindow = await _windowService.OpenAndGet<HudWindow>(WindowID.HUD);
            _hudWindow.BindButtons(OnRestartClicked, OnMenuClicked, StartBattle, OnSettingsClicked);
            _hudWindow.SetStartButtonVisible(false);
            
            _inputService.Enable();
            _inputService.OnCancelPerformed += HandleEsc;
            _inputService.OnClickPerformed += HandleClick;
            _inputService.OnHotbarHotkeyPressed += HandleHotbarInput;
            
            _plantingService.Initialize();
            _plantingService.OnPlantSelected += OnPlantSelected;
            _plantingService.OnPlantingSuccess += OnPlantingSuccess;
            
            _waveService.OnLevelCompleted += HandleVictory;

            if (_levelProvider.CurrentLevel != null)
                SubscribeToLevel();
            else
                _levelProvider.OnLevelLoaded += SubscribeToLevel;

            EnterPlantingMode();
            
            var levelData = _staticData.GetLevelData();
            if (levelData != null && levelData.levelMusic != null)
            {
                _audioService.InitializeMusicSource();
                _audioService.PlayMusic(levelData.levelMusic);
            }
            
            _windowService.Close(WindowID.Loading);
            
            StartTutorial();
        }

        private void StartTutorial()
        {
            _tutorialStep = 0;
            _hudWindow.SetDimmed(true);
            _hudWindow.ShowTutorialStep1_Selection();
        }

        private void OnPlantSelected(PlantType type)
        {
            if (_isTutorialComplete) return;

            // Шаг 2: Выбрано растение. Стрелка на Origin2 (Grid).
            if (type != PlantType.None && _tutorialStep == 0)
            {
                _tutorialStep = 1;
                _hudWindow.SetDimmed(false); 
                
                var data = _staticData.GetPlantData(type);
                if (data != null) _hudWindow.SetGhost(data.icon);
                
                _hudWindow.ShowTutorialStep2_Placement();
            }
            else if (type == PlantType.None)
            {
                _hudWindow.SetGhost(null);
            }
        }

        private void OnPlantingSuccess(Vector3 pos)
        {
            if (_isTutorialComplete) return;
            
            // Шаг 3: Растение посажено. Стрелка ВСЕ ЕЩЕ на Origin2.
            // Но мы подсвечиваем клетку, куда нужно нажать.
            if (_tutorialStep == 1)
            {
                _tutorialStep = 2;
                _hudWindow.SetGhost(null);
                
                // Стрелка остается на Origin2 (по запросу)
                _hudWindow.ShowTutorialStep3_Possession();

                if (_gridService.WorldToGrid(pos, out int l, out int r))
                {
                    // Подсвечиваем клетку на земле
                    _plantingService.ShowTutorialHighlight(l, r);
                }
            }
        }

        private async void EnterPossessionMode(IPossessablePlant plant)
        {
            // Финиш туториала
            if (!_isTutorialComplete && _tutorialStep == 2)
            {
                _isTutorialComplete = true;
                _hudWindow.HideTutorialArrow();
                _hudWindow.SetDimmed(false);
                _hudWindow.SetStartButtonVisible(true);
                
                // Убираем подсветку клетки
                _plantingService.HideTutorialHighlight();
            }
            
            if (!_isBattleStarted && _isTutorialComplete) StartBattle();

            _nextModeSwitchTime = Time.time + SWITCH_COOLDOWN;
            
            if (_currentPossessedPlant != null) _currentPossessedPlant.SetPossessed(false);
            
            _isInPossessionMode = true;
            _currentPossessedPlant = plant;
            
            _plantingService.ClearSelection();
            _hudWindow.SetGhost(null); 
            
            _hudWindow.SetGameplayVisibility(false);
            _hudWindow.SetActivePlant(plant);
            
            await _cameraService.MoveToTarget(plant.CameraMountPoint);
            plant.SetPossessed(true);
            
            if (!_windowService.IsWindowOpened(WindowID.Settings))
            {
                SetCursorState(false);
            }
        }
        
        // ... (Остальной код без изменений) ...
        
        public void Exit()
        {
            _windowService.Close(WindowID.HUD);
            if (_windowService.IsWindowOpened(WindowID.Settings))
                _windowService.Close(WindowID.Settings);
            
            _inputService.Disable();
            _inputService.OnCancelPerformed -= HandleEsc;
            _inputService.OnClickPerformed -= HandleClick;
            _inputService.OnHotbarHotkeyPressed -= HandleHotbarInput;
            
            _plantingService.OnPlantSelected -= OnPlantSelected;
            _plantingService.OnPlantingSuccess -= OnPlantingSuccess;
            _waveService.OnLevelCompleted -= HandleVictory;
            
            if (_levelProvider.CurrentLevel?.FinishTrigger != null)
                _levelProvider.CurrentLevel.FinishTrigger.OnZombieCrossed -= HandleDefeat;
            
            _levelProvider.OnLevelLoaded -= SubscribeToLevel;
            
            _plantingService.Dispose();
            
            SetCursorState(true);
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
        }

        private void HandleVictory()
        {
            if (_isGameOver) return;
            FinishGame(true);
        }

        private void HandleDefeat(ZombieController killer)
        {
            if (_isGameOver) return;
            
            var levelData = _staticData.GetLevelData();
            if (levelData != null && levelData.gameOverSound != null)
            {
                AudioSource.PlayClipAtPoint(levelData.gameOverSound, _levelProvider.CurrentLevel.OriginPosition, _audioService.SfxVolume);
            }
            
            FinishGame(false);
        }

        private void FinishGame(bool isVictory)
        {
            _isGameOver = true;
            _waveService.StopLevel();
            _inputService.Disable();
            
            if (_isInPossessionMode && _currentPossessedPlant != null)
                _currentPossessedPlant.SetPossessed(false);
            
            _cameraService.SetTacticalView(_levelProvider.CurrentLevel.CameraTacticalPoint);
            _hudWindow.SetGameplayVisibility(false);
            _hudWindow.SetActivePlant(null);
            _hudWindow.SetStartButtonVisible(false);
            
            SetCursorState(true);
            _hudWindow.ShowGameOverPanel(isVictory);
        }

        private void OnRestartClicked() => _stateMachine.ChangeState<GameLoadState>();
        private void OnMenuClicked() => _stateMachine.ChangeState<MainMenuState>();
        
        private void OnSettingsClicked()
        {
            if (!_windowService.IsWindowOpened(WindowID.Settings))
            {
                 _windowService.Open(WindowID.Settings);
                 SetCursorState(true);
            }
            else
            {
                _windowService.Close(WindowID.Settings);
                if (_isInPossessionMode) SetCursorState(false);
            }
        }

        private void HandleHotbarInput(int keyIndex)
        {
            if (_isGameOver || Time.time < _nextModeSwitchTime) return;
            int listIndex = keyIndex - 1;
            var plant = _plantTracker.GetPlantByIndex(listIndex);
            if (plant != null && _currentPossessedPlant != plant) EnterPossessionMode(plant);
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
            if (_isInPossessionMode)
            {
                EnterPlantingMode();
                return;
            }
            OnSettingsClicked();
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
            
            SetCursorState(true);
        }

        private void SetCursorState(bool isVisible)
        {
            Cursor.visible = isVisible;
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}