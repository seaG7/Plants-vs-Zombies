using System;
using Core.Interfaces;
using Data.Enums;
using Features.Plants;
using Features.Visuals;
using Infrastructure.Factories.Objects;
using Infrastructure.Providers.Context;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Input;
using UnityEngine;
using Zenject;

namespace Infrastructure.Services.Planting
{
    public class PlantingService : IPlantingService, IDisposable, ITickable
    {
        public event Action<PlantType> OnPlantSelected;
        public event Action<Vector3> OnPlantingSuccess;

        private readonly IInputService _inputService;
        private readonly IGridService _gridService;
        private readonly IEconomyService _economyService;
        private readonly IGameObjectFactory _factory;
        private readonly IStaticDataProvider _staticData;
        private readonly ILevelProvider _levelProvider;
        private readonly IPlantTrackerService _plantTracker;
        private readonly IAudioService _audioService;

        private PlantType _selectedPlantType = PlantType.None;
        private GridVisualizer _visualizer;

        public PlantingService(
            IInputService inputService, 
            IGridService gridService, 
            IEconomyService economyService,
            IGameObjectFactory factory,
            IStaticDataProvider staticData,
            ILevelProvider levelProvider,
            IPlantTrackerService plantTracker,
            IAudioService audioService)
        {
            _inputService = inputService;
            _gridService = gridService;
            _economyService = economyService;
            _factory = factory;
            _staticData = staticData;
            _levelProvider = levelProvider;
            _plantTracker = plantTracker;
            _audioService = audioService;
        }

        public void Initialize()
        {
            Dispose();

            var go = new GameObject("Planting_GridVisualizer");
            _visualizer = go.AddComponent<GridVisualizer>();
            _visualizer.Initialize();

            _levelProvider.OnLevelLoaded += OnLevelLoadedHandler;
            _inputService.OnCancelPerformed += ClearSelection;

            if (_levelProvider.CurrentLevel != null) 
                _visualizer.ShowLaneLines(_levelProvider);
        }

        public void Dispose()
        {
            _levelProvider.OnLevelLoaded -= OnLevelLoadedHandler;
            _inputService.OnCancelPerformed -= ClearSelection;

            if (_visualizer != null)
            {
                if (_visualizer.gameObject != null)
                    UnityEngine.Object.Destroy(_visualizer.gameObject);
                _visualizer = null;
            }
        }
        
        private void OnLevelLoadedHandler()
        {
            if (_visualizer != null && _levelProvider != null)
            {
                _visualizer.ShowLaneLines(_levelProvider);
            }
        }

        public void SelectPlant(PlantType type)
        {
            _selectedPlantType = type;
            OnPlantSelected?.Invoke(type);

            if (type != PlantType.None)
                _visualizer.ShowStaticGrid(_levelProvider, _gridService);
            else
                _visualizer.HideAllInteractive(); 
        }

        public void ClearSelection()
        {
            SelectPlant(PlantType.None);
        }

        public void Tick()
        {
            if (_selectedPlantType == PlantType.None) return;
            UpdateCursorHighlight();
        }

        private void UpdateCursorHighlight()
        {
            Vector2 mousePos = _inputService.GetPointerPosition();
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(mousePos);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 1000f,
                    LayerMask.GetMask("Default", "Ground")))
            {
                if (_gridService.WorldToGrid(hit.point, out int lane, out int row))
                {
                    bool isOccupied = _gridService.IsCellOccupied(lane, row);
                    Vector3 cellCenter = _gridService.GridToWorld(lane, row);
                    var level = _levelProvider.CurrentLevel;
                    
                    _visualizer.UpdateCursor(cellCenter, level.LaneWidth, level.CellLength, !isOccupied);
                    return;
                }
            }
            _visualizer.HideCursor();
        }

        public async void TryPlantAtCursor()
        {
            if (_selectedPlantType == PlantType.None) return;

            var plantData = _staticData.GetPlantData(_selectedPlantType);
            if (plantData == null) return;

            Vector2 mousePos = _inputService.GetPointerPosition();
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(mousePos);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 1000f,
                    LayerMask.GetMask("Default", "Ground")))
            {
                if (_gridService.WorldToGrid(hit.point, out int lane, out int row))
                {
                    if (_gridService.IsCellOccupied(lane, row)) return;

                    if (_economyService.TrySpendSun(plantData.cost))
                    {
                        Vector3 buildPos = _gridService.GridToWorld(lane, row);
                        GameObject plantObj = await _factory.InstantiateAsync(plantData.prefabReference, buildPos, Quaternion.identity);
                        
                        var possessable = plantObj.GetComponent<IPossessablePlant>();
                        if (possessable != null) 
                        {
                            possessable.Initialize(plantData);
                            _plantTracker.Register(possessable);
                        }
                        
                        var sunflower = plantObj.GetComponent<SunflowerController>();
                        if (sunflower != null)
                        {
                            sunflower.Initialize(plantData);
                        }
                        
                        if (plantData.plantSound != null)
                        {
                            AudioSource.PlayClipAtPoint(plantData.plantSound, buildPos, _audioService.SfxVolume);
                        }

                        _gridService.TryOccupyCell(lane, row, plantObj);
                        
                        OnPlantingSuccess?.Invoke(buildPos);
                        
                        _visualizer.ShowStaticGrid(_levelProvider, _gridService);
                        ClearSelection(); 
                    }
                }
            }
        }
        
        public void ShowTutorialHighlight(int lane, int row)
        {
             if (_visualizer == null || _levelProvider.CurrentLevel == null) return;
             
             Vector3 pos = _gridService.GridToWorld(lane, row);
             _visualizer.ShowTutorialHighlight(pos, _levelProvider.CurrentLevel.LaneWidth, _levelProvider.CurrentLevel.CellLength);
        }

        public void HideTutorialHighlight()
        {
            if (_visualizer != null)
                _visualizer.HideTutorialHighlight();
        }
    }
}