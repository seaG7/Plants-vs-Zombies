using System;
using Data.Enums;
using Features.Cannon;
using Infrastructure.Factories.Objects;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Input;
using UnityEngine;
using Zenject;

namespace Infrastructure.Services.Planting
{
    public class PlantingService : IPlantingService, IInitializable, IDisposable
    {
        private readonly IInputService _inputService;
        private readonly IGridService _gridService;
        private readonly IEconomyService _economyService;
        private readonly IGameObjectFactory _factory;
        private readonly IStaticDataProvider _staticData;

        private PlantType _selectedPlantType = PlantType.None;

        public PlantingService(
            IInputService inputService, 
            IGridService gridService, 
            IEconomyService economyService,
            IGameObjectFactory factory,
            IStaticDataProvider staticData)
        {
            _inputService = inputService;
            _gridService = gridService;
            _economyService = economyService;
            _factory = factory;
            _staticData = staticData;
        }

        public void Initialize()
        {
            _inputService.OnHotbarHotkeyPressed += OnHotbarPressed;
            _inputService.OnCancelPerformed += Deselect;
        }

        public void Dispose()
        {
            _inputService.OnHotbarHotkeyPressed -= OnHotbarPressed;
            _inputService.OnCancelPerformed -= Deselect;
        }

        private void OnHotbarPressed(int slotIndex)
        {
            if (slotIndex == 1) SelectPlant(PlantType.CoconutCannon);
            else if (slotIndex == 2) SelectPlant(PlantType.Peashooter);
        }

        public void SelectPlant(PlantType type)
        {
            _selectedPlantType = type;
            Debug.Log($"[PlantingService] Selected: {type}");
        }
        
        private void Deselect()
        {
            _selectedPlantType = PlantType.None;
        }

        public async void TryPlantAtCursor()
        {
            if (_selectedPlantType == PlantType.None) return;

            var plantData = _staticData.GetPlantData(_selectedPlantType);
            if (plantData == null) return;

            Vector2 mousePos = _inputService.GetPointerPosition();
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(mousePos);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Default", "Ground")))
            {
                if (_gridService.WorldToGrid(hit.point, out int lane, out int row))
                {
                    if (_gridService.IsCellOccupied(lane, row)) return;

                    if (_economyService.TrySpendSun(plantData.cost))
                    {
                        Vector3 buildPos = _gridService.GridToWorld(lane, row);
                        GameObject plantObj = await _factory.InstantiateAsync(plantData.prefabReference, buildPos, Quaternion.identity);

                        var cannon = plantObj.GetComponent<CannonController>();
                        if (cannon != null)
                        {
                            cannon.Initialize(plantData);
                        }
                        
                        _gridService.TryOccupyCell(lane, row, plantObj);

                        _selectedPlantType = PlantType.None; 
                    }
                    else
                    {
                        Debug.Log("Not enough sun!");
                    }
                }
            }
        }
    }
}