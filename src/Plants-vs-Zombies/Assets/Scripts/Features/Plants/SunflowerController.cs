using Data.Configs;
using Infrastructure.Services.Economy;
using UnityEngine;
using Zenject;

namespace Features.Plants
{
    public class SunflowerController : MonoBehaviour
    {
        private IEconomyService _economyService;
        private PlantData _config;
        private float _timer;
        private bool _isInitialized;

        [Inject]
        public void Construct(IEconomyService economyService)
        {
            _economyService = economyService;
        }

        public void Initialize(PlantData config)
        {
            _config = config;
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            _timer += Time.deltaTime;
            if (_timer >= _config.sunGenerationInterval)
            {
                _timer = 0f;
                _economyService.AddSun(_config.sunGenerationAmount);
            }
        }
    }
}