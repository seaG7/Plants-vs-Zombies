using System;
using Data.Enums;
using Features.Cannon;
using Features.Visuals;
using Infrastructure.Factories.Objects;
using Infrastructure.Providers.Context;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Input;
using UnityEngine;
using Zenject;

namespace Infrastructure.Services.Planting
{
    public class PlantingService : IPlantingService, IInitializable, IDisposable, ITickable
    {
        public event Action<PlantType> OnPlantSelected;

        private readonly IInputService _inputService;
        private readonly IGridService _gridService;
        private readonly IEconomyService _economyService;
        private readonly IGameObjectFactory _factory;
        private readonly IStaticDataProvider _staticData;
        private readonly ILevelProvider _levelProvider;

        private PlantType _selectedPlantType = PlantType.None;
        private GridVisualizer _visualizer;

        public PlantingService(
            IInputService inputService, 
            IGridService gridService, 
            IEconomyService economyService,
            IGameObjectFactory factory,
            IStaticDataProvider staticData,
            ILevelProvider levelProvider)
        {
            _inputService = inputService;
            _gridService = gridService;
            _economyService = economyService;
            _factory = factory;
            _staticData = staticData;
            _levelProvider = levelProvider;
        }

        public void Initialize()
        {
            var go = new GameObject("Planting_GridVisualizer");
            _visualizer = go.AddComponent<GridVisualizer>();
            _visualizer.Initialize();
            
            _levelProvider.OnLevelLoaded += () => _visualizer.ShowLaneLines(_levelProvider);
            if (_levelProvider.CurrentLevel != null) _visualizer.ShowLaneLines(_levelProvider);

            _inputService.OnHotbarHotkeyPressed += OnHotbarPressed;
            _inputService.OnCancelPerformed += ClearSelection;
        }

        public void Dispose()
        {
            _levelProvider.OnLevelLoaded -= () => _visualizer.ShowLaneLines(_levelProvider);
            _inputService.OnHotbarHotkeyPressed -= OnHotbarPressed;
            _inputService.OnCancelPerformed -= ClearSelection;
            if (_visualizer != null) UnityEngine.Object.Destroy(_visualizer.gameObject);
        }

        private void OnHotbarPressed(int slotIndex)
        {
            if (slotIndex == 1) SelectPlant(PlantType.CoconutCannon);
            else if (slotIndex == 2) SelectPlant(PlantType.Peashooter);
        }

        public void SelectPlant(PlantType type)
        {
            _selectedPlantType = type;
            OnPlantSelected?.Invoke(type);

            if (type != PlantType.None)
                _visualizer.ShowStaticGrid(_levelProvider, _gridService);
            else
                _visualizer.HideAllInteractive(); // Прячем курсор и ячейки, но НЕ линии лайнов
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
                        
                        var cannon = plantObj.GetComponent<CannonController>();
                        if (cannon != null) cannon.Initialize(plantData);
                        
                        _gridService.TryOccupyCell(lane, row, plantObj);
                        
                        _visualizer.ShowStaticGrid(_levelProvider, _gridService);
                        ClearSelection(); 
                    }
                }
            }
        }
    }
}