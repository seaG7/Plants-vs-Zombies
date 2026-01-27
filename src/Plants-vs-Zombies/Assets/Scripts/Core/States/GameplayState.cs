using Core.BaseStates;
using Features.Cannon;
using Infrastructure.Providers.Context;
using Infrastructure.Services;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Input;
using Infrastructure.Services.Planting;
using Infrastructure.Services.Waves;
using Infrastructure.Services.Window;
using UI.HUD;
using UnityEngine;

namespace Core.States
{
    public class GameplayState : IState, IEnterable, IExitable
    {
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly ILevelProvider _levelProvider;
        private readonly IPlantingService _plantingService;
        private readonly IGridService _gridService;

        private HudWindow _hudWindow;
        private CannonController _currentPossessedPlant;
        private bool _isInPossessionMode;
        private bool _waveInProgress = false;
        
        private float _nextModeSwitchTime;
        private readonly IWaveService _waveService;
        private const float SWITCH_COOLDOWN = 0.5f;

        public GameplayState(
            IWindowService windowService, 
            IInputService inputService, 
            ICameraService cameraService,
            ILevelProvider levelProvider,
            IPlantingService plantingService,
            IGridService gridService,
            IWaveService waveService)
        {
            _windowService = windowService;
            _inputService = inputService;
            _cameraService = cameraService;
            _levelProvider = levelProvider;
            _plantingService = plantingService;
            _gridService = gridService;
            _waveService = waveService;
        }

        public async void Enter()
        {
            _hudWindow = await _windowService.OpenAndGet<HudWindow>(WindowID.HUD);
            
            _inputService.Enable();
            _inputService.OnCancelPerformed += HandleEsc;
            _inputService.OnClickPerformed += HandleClick;
            
            _plantingService.Initialize();
            EnterPlantingMode();
        }

        public void Exit()
        {
            _windowService.Close(WindowID.HUD);
            _inputService.Disable();
            _inputService.OnCancelPerformed -= HandleEsc;
            _inputService.OnClickPerformed -= HandleClick;
        }

        private void HandleClick()
        {
            if (Time.time < _nextModeSwitchTime) return;

            if (_isInPossessionMode) return;

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
                    else
                    {
                        _plantingService.TryPlantAtCursor();
                    }
                }
            }
        }

        private void HandleEsc()
        {
            if (Time.time < _nextModeSwitchTime) return;

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
            
            _hudWindow.SetActiveCannon(null);

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

        private async void EnterPossessionMode(CannonController plant)
        {
            _nextModeSwitchTime = Time.time + SWITCH_COOLDOWN;
            _isInPossessionMode = true;
            _currentPossessedPlant = plant;
            
            _plantingService.ClearSelection();
            
            _hudWindow.SetGameplayVisibility(false);
            _hudWindow.SetActiveCannon(plant);
            
            await _cameraService.MoveToTarget(plant.CameraMountPoint);
            
            plant.SetPossessed(true);

            if (!_waveInProgress) _waveService.StartLevel();
        }
    }
}