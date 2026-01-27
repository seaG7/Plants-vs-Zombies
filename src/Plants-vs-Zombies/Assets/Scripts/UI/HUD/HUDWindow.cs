using System;
using System.Collections.Generic;
using Core.Interfaces;
using Data.Configs;
using Features.Plants;
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

        [Header("Active Plants UI")]
        [SerializeField] private Transform _activePlantsContainer;
        [SerializeField] private ActivePlantView _activePlantPrefab;
        
        [Header("Game Over UI")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        
        [Header("Battle Controls")]
        [SerializeField] private Button _startBattleButton;


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
        private IPlantTrackerService _plantTracker;
        private DiContainer _container;

        private readonly List<PlantCard> _cards = new();
        private float _lastUpdateTimer;
        private IPossessablePlant _activePlant;
        private float _warningTimer;

        [Inject]
        public void Construct(
            IFPSService fpsService, 
            IEconomyService economyService,
            IStaticDataProvider staticData,
            IPlantingService plantingService,
            IPlantTrackerService plantTracker,
            DiContainer container)
        {
            _fpsService = fpsService;
            _economyService = economyService;
            _staticData = staticData;
            _plantingService = plantingService;
            _plantTracker = plantTracker;
            _container = container;
        }

        private void Start()
        {
            InitializeCards();
            _gameOverPanel.SetActive(false);
            
            _economyService.OnSunChanged += UpdateSunDisplay;
            UpdateSunDisplay(_economyService.CurrentSun);
            
            _plantingService.OnPlantSelected += HandlePlantSelected;
            _plantTracker.OnListChanged += RebuildActivePlantsList;

            ToggleActionPanel(false);
            RebuildActivePlantsList();
        }

        private void OnDestroy()
        {
            _economyService.OnSunChanged -= UpdateSunDisplay;
            _plantingService.OnPlantSelected -= HandlePlantSelected;
            if (_plantTracker != null) _plantTracker.OnListChanged -= RebuildActivePlantsList;
            UnsubscribeActivePlant();
        }
        
        public void BindButtons(Action onRestart, Action onMenu, Action onStartBattle) // Updated signature
        {
            _restartButton.onClick.AddListener(() => onRestart?.Invoke());
            _menuButton.onClick.AddListener(() => onMenu?.Invoke());
            _startBattleButton.onClick.AddListener(() => onStartBattle?.Invoke());
        }

        public void ShowGameOverPanel()
        {
            _gameOverPanel.SetActive(true);
        }

        public void SetActivePlant(IPossessablePlant plant)
        {
            UnsubscribeActivePlant();
            _activePlant = plant;

            if (_activePlant != null)
            {
                ToggleActionPanel(true);
                _activePlant.OnFireFailedCooldown += ShowCooldownWarning;
            }
            else
            {
                ToggleActionPanel(false);
            }
            RebuildActivePlantsList();
        }

        private void UnsubscribeActivePlant()
        {
            if (_activePlant != null)
            {
                _activePlant.OnFireFailedCooldown -= ShowCooldownWarning;
                _activePlant = null;
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
            UpdatePlantUI();
        }

        private void UpdatePlantUI()
        {
            if (_activePlant == null || !_actionPanel.activeSelf) return;
            float progress = _activePlant.GetReloadProgress();
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
        
        private void RebuildActivePlantsList()
        {
            foreach (Transform child in _activePlantsContainer) Destroy(child.gameObject);
            var plants = _plantTracker.GetAll();
            for (int i = 0; i < plants.Count; i++)
            {
                var plant = plants[i];
                var view = _container.
                    InstantiatePrefabForComponent<ActivePlantView>(_activePlantPrefab, _activePlantsContainer);
                Data.Enums.PlantType type = Data.Enums.PlantType.None;
                if (plant is CannonController) type = Data.Enums.PlantType.CoconutCannon;
                else if (plant is PeashooterController) type = Data.Enums.PlantType.Peashooter;
                view.Initialize(plant, i, type);
                view.SetSelected(_activePlant == plant);
            }
        }

        private void OnCardClicked(PlantData data) => _plantingService.SelectPlant(data.type);
        private void HandlePlantSelected(Data.Enums.PlantType type) { }
        private void UpdateSunDisplay(int amount) => _sunText.text = $"{amount}";

        public void SetGameplayVisibility(bool isVisible) => _gameplayPanel.SetActive(isVisible);
        
        private void ToggleActionPanel(bool isActive)
        {
            if (_actionPanel != null) _actionPanel.SetActive(isActive);
        }
        public void SetStartButtonVisible(bool isVisible)
        {
            if (_startBattleButton != null) 
                _startBattleButton.gameObject.SetActive(isVisible);
        }
    }
}