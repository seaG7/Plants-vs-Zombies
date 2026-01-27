using System;
using System.Collections.Generic;
using Core.Interfaces;

namespace Infrastructure.Services.Planting
{
    /// <summary>
    /// Manages the list of active possessable plants for hotkey switching.
    /// </summary>
    public class PlantTrackerService : IPlantTrackerService
    {
        private readonly List<IPossessablePlant> _activePlants = new();

        public event Action OnListChanged;
        public List<IPossessablePlant> GetAll() => _activePlants;


        public void Register(IPossessablePlant plant)
        {
            if (!_activePlants.Contains(plant))
            {
                _activePlants.Add(plant);
                OnListChanged?.Invoke();
            }
        }

        public void Unregister(IPossessablePlant plant)
        {
            if (_activePlants.Contains(plant))
            {
                _activePlants.Remove(plant);
                OnListChanged?.Invoke();
            }
        }

        public IPossessablePlant GetPlantByIndex(int index)
        {
            if (index >= 0 && index < _activePlants.Count)
            {
                return _activePlants[index];
            }
            return null;
        }

        public void Clear()
        {
            _activePlants.Clear();
            OnListChanged?.Invoke();
        }
    }
}