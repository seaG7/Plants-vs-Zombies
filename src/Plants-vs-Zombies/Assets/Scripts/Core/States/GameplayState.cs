using Core.BaseStates;
using Core.Interfaces;
using Data.Enums;
using Features.Enemy;
using Infrastructure.Providers.Context;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Input;
using Infrastructure.Services.Planting;
using Infrastructure.Services.Waves;
using Infrastructure.Services.Window;
using Infrastructure.Services.Yandex;
using UI.HUD;
using UnityEngine;
using YG;

namespace Core.States
{
    public class GameplayState : IState, IEnterable, IExitable
    {
        // ... (поля остаются без изменений)
        private readonly StateMachine _stateMachine;
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly ILevelProvider _levelProvider;
        private readonly IPlantingService _plantingService;
        private readonly IGridService _gridService;
        private readonly IWaveService _waveService;
        private readonly IPlantTrackerService _plantTracker;
        private readonly IEconomyService _economyService;
        private readonly IStaticDataProvider _staticData;
        private readonly IAudioService _audioService;
        private readonly IYandexService _yandexService;

        private HudWindow _hudWindow;
        private IPossessablePlant _currentPossessedPlant;
        private bool _isInPossessionMode;
        private bool _isBattleStarted;
        private bool _isGameOver;
        private float _nextModeSwitchTime;
        private const float SWITCH_COOLDOWN = 0.7f;
        
        private bool _isTutorialComplete = false;
        private int _tutorialStep = 0; 

        // Constructor... (без изменений)
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
            IEconomyService economyService,
            IStaticDataProvider staticData,
            IAudioService audioService,
            IYandexService yandexService)
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
            _economyService = economyService;
            _staticData = staticData;
            _audioService = audioService;
            _yandexService = yandexService;
        }

        public async void Enter()
        {
            _isGameOver = false;
            _isBattleStarted = false; 
            _isTutorialComplete = false;
            _tutorialStep = 0;
            
            _hudWindow = await _windowService.OpenAndGet<HudWindow>(WindowID.HUD);
            _hudWindow.BindButtons(OnRestartClicked, OnMenuClicked, StartBattle, OnSettingsClicked);
            
            _inputService.Enable();
            _inputService.OnCancelPerformed += HandleEsc;
            _inputService.OnClickPerformed += HandleClick;
            _inputService.OnHotbarHotkeyPressed += HandleHotbarInput;
            
            _plantingService.Initialize();
            _plantingService.OnPlantSelected += OnPlantSelected;
            _plantingService.OnPlantingSuccess += OnPlantingSuccess;
            
            _waveService.OnLevelCompleted += HandleVictory;

            if (_levelProvider.CurrentLevel != null) SubscribeToLevel();
            else _levelProvider.OnLevelLoaded += SubscribeToLevel;

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

        // ... (StartTutorial, OnPlantSelected, OnPlantingSuccess, EnterPossessionMode - без изменений логики, только SetCursorState)

        private void StartTutorial()
        {
            _tutorialStep = 0;
            _hudWindow.SetDimmed(true);
            _hudWindow.ShowTutorialStep1_Selection();
        }

        private void OnPlantSelected(PlantType type)
        {
            if (_isTutorialComplete) return;
            if (type != PlantType.None && _tutorialStep == 0)
            {
                _tutorialStep = 1;
                _hudWindow.SetDimmed(false); 
                var data = _staticData.GetPlantData(type);
                if (data != null) _hudWindow.SetGhost(data.icon);
                _hudWindow.ShowTutorialStep2_Placement();
            }
            else if (type == PlantType.None) _hudWindow.SetGhost(null);
        }

        private void OnPlantingSuccess(Vector3 pos)
        {
            if (_isTutorialComplete) return;
            if (_tutorialStep == 1)
            {
                _tutorialStep = 2;
                _hudWindow.SetGhost(null);
                _hudWindow.ShowTutorialStep3_Possession();
                if (_gridService.WorldToGrid(pos, out int l, out int r)) _plantingService.ShowTutorialHighlight(l, r);
            }
        }

        private async void EnterPossessionMode(IPossessablePlant plant)
        {
            if (!_isTutorialComplete && _tutorialStep == 2)
            {
                _isTutorialComplete = true;
                _hudWindow.HideTutorialArrow();
                _hudWindow.SetDimmed(false);
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
            _hudWindow.SetActivePlant(plant); // Это включит джойстик на мобиле
            
            await _cameraService.MoveToTarget(plant.CameraMountPoint);
            plant.SetPossessed(true);
            
            if (!_windowService.IsWindowOpened(WindowID.Settings))
                SetCursorState(false);
        }

        public void Exit()
        {
            _windowService.Close(WindowID.HUD);
            if (_windowService.IsWindowOpened(WindowID.Settings)) _windowService.Close(WindowID.Settings);
            
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
            _waveService.StartLevel();
        }

        private void HandleVictory() => FinishGame(true);
        private void HandleDefeat(ZombieController killer) => FinishGame(false);

        private void FinishGame(bool isVictory)
        {
            if (_isGameOver) return;
            _isGameOver = true;
            _waveService.StopLevel();
            _inputService.Disable();
            
            if (_isInPossessionMode && _currentPossessedPlant != null)
                _currentPossessedPlant.SetPossessed(false);
            
            int currentSuns = _economyService.CurrentSun;
            _yandexService.AddGold(currentSuns); 

            var levelData = _staticData.GetLevelData();
            if (!isVictory && levelData != null && levelData.gameOverSound != null)
                AudioSource.PlayClipAtPoint(levelData.gameOverSound, _levelProvider.CurrentLevel.OriginPosition, _audioService.SfxVolume);

            _cameraService.SetTacticalView(_levelProvider.CurrentLevel.CameraTacticalPoint);
            _hudWindow.SetGameplayVisibility(false);
            _hudWindow.SetActivePlant(null);
            
            SetCursorState(true);
            _hudWindow.ShowGameOverPanel(isVictory);
        }

        private void OnRestartClicked()
        {
            _yandexService.ShowInterstitial(() => _stateMachine.ChangeState<GameLoadState>());
        }

        private void OnMenuClicked()
        {
            _yandexService.ShowInterstitial(() => _stateMachine.ChangeState<MainMenuState>());
        }

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

            // Если нажали на UI (через EventSystem), игнорируем рейкаст в мир
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                // Если это мобилка, IsPointerOverGameObject требует ID тача, но базовая проверка обычно работает для первого тача
                // Однако, если мы нажимаем "в пустоту" (не на кнопку), то это не считается UI кликом для блокировки логики, 
                // НО мы хотим сбросить выделение если это не кнопка.
                // В Unity UI "пустота" между кнопками обычно пропускает клик сквозь (raycast target off на панелях).
                // Поэтому если клик прошел сквозь UI в HandleClick, значит мы кликнули либо в мир, либо в пустоту.
                return; 
            }

            if (_isInPossessionMode) return;
            
            Vector2 mousePos = _inputService.GetPointerPosition();
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(mousePos);

            bool hitWorld = UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Default", "Ground"));

            if (hitWorld)
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
                    return; // Успешное взаимодействие с сеткой
                }
            }
            
            // Если мы здесь - значит кликнули в "молоко" (мимо сетки или в небо)
            // Аннулируем выбранное растение
            _plantingService.ClearSelection();
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
            if (!_isInPossessionMode && _currentPossessedPlant == null) _plantingService.ClearSelection();
            else _nextModeSwitchTime = Time.time + SWITCH_COOLDOWN;
            
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
            // !!! FIX: На мобилке курсор всегда должен быть доступен (unlock),
            // иначе джойстик и кнопки перестают реагировать на тачи.
            if (YG2.envir.isMobile || Application.isMobilePlatform)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return;
            }

            // Логика для ПК
            Cursor.visible = isVisible;
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}