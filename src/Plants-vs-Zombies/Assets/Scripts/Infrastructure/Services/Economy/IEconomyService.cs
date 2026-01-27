using System;

namespace Infrastructure.Services.Economy
{
    public interface IEconomyService
    {
        int CurrentSun { get; }
        event Action<int> OnSunChanged;

        void AddSun(int amount);
        bool TrySpendSun(int amount);
        void StartPassiveIncome();
        void StopPassiveIncome();
        void Reset();
    }
}