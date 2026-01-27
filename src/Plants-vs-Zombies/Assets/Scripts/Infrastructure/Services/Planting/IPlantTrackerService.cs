using System;
using System.Collections.Generic;
using Core.Interfaces;

namespace Infrastructure.Services.Planting
{
    public interface IPlantTrackerService
    {
        event Action OnListChanged;
        void Register(IPossessablePlant plant);
        void Unregister(IPossessablePlant plant);
        IPossessablePlant GetPlantByIndex(int index);
        List<IPossessablePlant> GetAll();
        void Clear();
    }
}