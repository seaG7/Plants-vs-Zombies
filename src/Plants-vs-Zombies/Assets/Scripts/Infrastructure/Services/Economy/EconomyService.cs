using System;
using Data.Configs;
using Infrastructure.Providers.StaticData;
using UnityEngine;
using Zenject;

namespace Infrastructure.Services.Economy
{
    public class EconomyService : IEconomyService, ITickable
    {
        private readonly IStaticDataProvider _staticData;
        
        private EconomySettings _settings;
        private int _currentSun;
        private float _timer;
        private bool _isIncomeActive;

        public int CurrentSun => _currentSun;
        public event Action<int> OnSunChanged;

        public EconomyService(IStaticDataProvider staticData)
        {
            _staticData = staticData;
        }

        public void Initialize()
        {
            var levelConfig = _staticData.GetLevelData();
            if (levelConfig != null)
            {
                _settings = levelConfig.economy;
                _currentSun = _settings.startingSun;
                OnSunChanged?.Invoke(_currentSun);
            }
        }

        public void StartPassiveIncome() => _isIncomeActive = true;
        public void StopPassiveIncome() => _isIncomeActive = false;

        public void AddSun(int amount)
        {
            _currentSun += amount;
            OnSunChanged?.Invoke(_currentSun);
        }

        public bool TrySpendSun(int amount)
        {
            if (_currentSun >= amount)
            {
                _currentSun -= amount;
                OnSunChanged?.Invoke(_currentSun);
                return true;
            }
            return false;
        }

        public void Tick()
        {
            if (!_isIncomeActive || _settings == null) return;

            _timer += Time.deltaTime;
            if (_timer >= _settings.passiveIncomeInterval)
            {
                AddSun(_settings.passiveIncomeAmount);
                _timer = 0f;
            }
        }
        
        public void Reset()
        {
            _isIncomeActive = false;
            _timer = 0f;
            Initialize();
        }
    }
}