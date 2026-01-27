using System.Collections.Generic;
using Data.Configs;
using Features.Cannon;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services.Economy;
using Infrastructure.Services.FPS;
using Infrastructure.Services.Planting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.HUD
{
    public class HudWindow : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _fpsText;
        [SerializeField] private TextMeshProUGUI _sunText;
        
        [Header("Planting UI")]
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private PlantCard _cardPrefab;
        [SerializeField] private GameObject _gameplayPanel;

        [Header("Action Mode UI")]
        [SerializeField] private GameObject _actionPanel;
        [SerializeField] private Slider _cooldownSlider;
        [SerializeField] private TextMeshProUGUI _cooldownStatusText;
        
        [Header("Text Config")]
        [SerializeField] private string _textReady = "СНАРЯД ГОТОВ";
        [SerializeField] private string _textReloading = "ПЕРЕЗАРЯДКА...";
        [SerializeField] private string _textNotReadyWarning = "Снаряд ещё не готов";
        [SerializeField] private Color _colorReady = Color.green;
        [SerializeField] private Color _colorNotReady = Color.red;

        private IFPSService _fpsService;
        private IEconomyService _economyService;
        private IStaticDataProvider _staticData;
        private IPlantingService _plantingService;

        private readonly List<PlantCard> _cards = new();
        private float _lastUpdateTimer;
        
        private CannonController _activeCannon;
        private float _warningTimer;

        [Inject]
        public void Construct(
            IFPSService fpsService, 
            IEconomyService economyService,
            IStaticDataProvider staticData,
            IPlantingService plantingService)
        {
            _fpsService = fpsService;
            _economyService = economyService;
            _staticData = staticData;
            _plantingService = plantingService;
        }

        private void Start()
        {
            InitializeCards();
            
            _economyService.OnSunChanged += UpdateSunDisplay;
            UpdateSunDisplay(_economyService.CurrentSun);
            
            _plantingService.OnPlantSelected += HandlePlantSelected;

            ToggleActionPanel(false);
        }

        private void OnDestroy()
        {
            _economyService.OnSunChanged -= UpdateSunDisplay;
            _plantingService.OnPlantSelected -= HandlePlantSelected;
            UnsubscribeActiveCannon();
        }

        public void SetActiveCannon(CannonController cannon)
        {
            UnsubscribeActiveCannon();
            _activeCannon = cannon;

            if (_activeCannon != null)
            {
                ToggleActionPanel(true);
                _activeCannon.OnFireFailedCooldown += ShowCooldownWarning;
            }
            else
            {
                ToggleActionPanel(false);
            }
        }

        private void UnsubscribeActiveCannon()
        {
            if (_activeCannon != null)
            {
                _activeCannon.OnFireFailedCooldown -= ShowCooldownWarning;
                _activeCannon = null;
            }
        }

        private void ShowCooldownWarning()
        {
            _cooldownStatusText.text = _textNotReadyWarning;
            _cooldownStatusText.color = _colorNotReady;
            _warningTimer = 1.0f;
        }

        private void Update()
        {
            UpdateFPS();
            UpdateCardsAvailability();
            UpdateCannonUI();
        }

        private void UpdateCannonUI()
        {
            if (_activeCannon == null || !_actionPanel.activeSelf) return;

            float progress = _activeCannon.GetReloadProgress();
            _cooldownSlider.value = progress;
            bool isReady = progress >= 0.99f;
            
            if (isReady)
            {
                _warningTimer = 0f;
                _cooldownStatusText.text = _textReady;
                _cooldownStatusText.color = _colorReady;
            }
            else if (_warningTimer > 0)
            {
                _warningTimer -= Time.deltaTime;
            }
            else
            {
                _cooldownStatusText.text = _textReloading;
                _cooldownStatusText.color = Color.white;
            }
        }

        private void UpdateFPS()
        {
            _lastUpdateTimer += Time.deltaTime;
            if (_lastUpdateTimer >= 0.5f)
            {
                _fpsText.text = $"{_fpsService.CurrentFps:F0}";
                _lastUpdateTimer = 0f;
            }
        }
        
        private void UpdateCardsAvailability()
        {
            int currentSun = _economyService.CurrentSun;
            var plants = _staticData.GetAllPlants();
            for (int i = 0; i < _cards.Count && i < plants.Count; i++)
            {
                _cards[i].SetAffordable(currentSun >= plants[i].cost);
            }
        }

        private void InitializeCards()
        {
            foreach (Transform child in _cardsContainer) Destroy(child.gameObject);
            _cards.Clear();

            var plants = _staticData.GetAllPlants();
            foreach (var plantData in plants)
            {
                var cardInstance = Instantiate(_cardPrefab, _cardsContainer);
                cardInstance.Initialize(plantData, OnCardClicked);
                _cards.Add(cardInstance);
            }
        }

        private void OnCardClicked(PlantData data) => _plantingService.SelectPlant(data.type);
        private void HandlePlantSelected(Data.Enums.PlantType type) { /* Visual selection logic */ }
        private void UpdateSunDisplay(int amount) => _sunText.text = $"{amount}";

        public void SetGameplayVisibility(bool isVisible) => _gameplayPanel.SetActive(isVisible);
        
        private void ToggleActionPanel(bool isActive)
        {
            if (_actionPanel != null) _actionPanel.SetActive(isActive);
        }
    }
}