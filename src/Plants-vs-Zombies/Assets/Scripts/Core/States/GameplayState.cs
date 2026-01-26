using Core.BaseStates;
using Features.Cannon;
using Infrastructure.Providers.Context;
using Infrastructure.Services;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Input;
using Infrastructure.Services.Planting;
using Infrastructure.Services.Waves;
using Infrastructure.Services.Window;
using Infrastructure.Services.Grid;
using UI.HUD;
using UnityEngine;

namespace Core.States
{
    /// <summary>
    /// Manages the main gameplay loop, switching between Tactical Planting and Third-Person Possession.
    /// </summary>
    public class GameplayState : IState, IEnterable, IExitable
    {
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private readonly IWaveService _waveService;
        private readonly ICameraService _cameraService;
        private readonly ILevelProvider _levelProvider;
        private readonly IPlantingService _plantingService;
        private readonly IGridService _gridService;

        private HudWindow _hudWindow;
        private CannonController _currentPossessedPlant;
        private bool _isWaveActive;

        public GameplayState(
            IWindowService windowService, 
            IInputService inputService, 
            IWaveService waveService,
            ICameraService cameraService,
            ILevelProvider levelProvider,
            IPlantingService plantingService,
            IGridService gridService)
        {
            _windowService = windowService;
            _inputService = inputService;
            _waveService = waveService;
            _cameraService = cameraService;
            _levelProvider = levelProvider;
            _plantingService = plantingService;
            _gridService = gridService;
        }

        public async void Enter()
        {
            _hudWindow = await _windowService.OpenAndGet<HudWindow>(WindowID.HUD);
            _hudWindow.OnStartWaveClicked += StartWavePhase;
            
            _inputService.Enable();
            _inputService.OnCancelPerformed += HandleEsc;
            _inputService.OnClickPerformed += HandleClick;
            _waveService.OnWaveStarted += OnWaveStarted;
            
            EnterTacticalMode();
        }

        public void Exit()
        {
            if (_hudWindow != null) 
                _hudWindow.OnStartWaveClicked -= StartWavePhase;

            _windowService.Close(WindowID.HUD);
            
            _inputService.Disable();
            _inputService.OnCancelPerformed -= HandleEsc;
            _inputService.OnClickPerformed -= HandleClick;
            _waveService.OnWaveStarted -= OnWaveStarted;
        }

        private void StartWavePhase()
        {
            if (_isWaveActive) return;
            
            _isWaveActive = true;
            _hudWindow.SetStartButtonVisible(false);
            _waveService.StartLevel();
        }

        private void OnWaveStarted(int waveIndex)
        {
            _hudWindow.SetWaveInfo(waveIndex);
        }

        private void HandleClick()
        {
            if (_currentPossessedPlant != null) return;

            if (_currentPossessedPlant != null) return;

            Vector2 mousePos = _inputService.GetPointerPosition();
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(mousePos);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                if (_gridService.WorldToGrid(hit.point, out int lane, out int row))
                {
                    if (_gridService.IsCellOccupied(lane, row))
                    {
                        var plantObj = _gridService.GetPlantAt(lane, row);
                        var cannon = plantObj.GetComponent<CannonController>();
                        if (cannon != null)
                        {
                            EnterPossessionMode(cannon);
                        }
                    }
                    else if (!_isWaveActive) 
                    {
                        _plantingService.TryPlantAtCursor();
                    }
                }
            }
        }

        private void HandleEsc()
        {
            if (_currentPossessedPlant != null)
            {
                EnterTacticalMode();
            }
        }

        private async void EnterTacticalMode()
        {
            if (_currentPossessedPlant != null)
            {
                _currentPossessedPlant.SetPossessed(false);
                _currentPossessedPlant = null;
            }

            _cameraService.SetTacticalView(_levelProvider.CurrentLevel.CameraTacticalPoint);
            _hudWindow.ToggleControls(true);
            
            if (!_isWaveActive) 
                _hudWindow.SetStartButtonVisible(true);
        }

        private async void EnterPossessionMode(CannonController plant)
        {
            _currentPossessedPlant = plant;
            _currentPossessedPlant.SetPossessed(true);
            
            _hudWindow.ToggleControls(false); 
            
            await _cameraService.MoveToTarget(plant.CameraMountPoint);
        }
    }
}