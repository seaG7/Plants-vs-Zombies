// ===== UI/HUD/HudWindow.cs =====
using System;
using System.Collections.Generic;
using Core.Interfaces;
using Data.Configs;
using Data.Enums;
using DG.Tweening;
using Features.Plants;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Input;
using Infrastructure.Services.Planting;
using Infrastructure.Services.Yandex;
using TMPro;
using UI.Mobile;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.HUD
{
    /// <summary>
    /// Main Gameplay UI. Handles HUD modes (Planting vs Action), Mobile Controls visibility and Localization.
    /// </summary>
    public class HudWindow : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _sunText;
        
        [Header("Global Controls")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitToMenuButton;

        [Header("Controls Mobile")]
        [SerializeField] private MobileControlsView _mobileControls;
        
        [Header("Planting UI")]
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private PlantCard _cardPrefab;
        [SerializeField] private GameObject _gameplayPanel; // Contains Plant Cards
        [SerializeField] private Image _plantingGhostImage; 

        [Header("Tutorial UI")]
        [SerializeField] private GameObject _dimmedPanel; 
        [SerializeField] private RectTransform _tutorialArrow;
        [SerializeField] private RectTransform _tutorialOrigin1; 
        [SerializeField] private RectTransform _tutorialOrigin2; 
        [SerializeField] private float _arrowAnimDuration = 0.6f;

        [Header("Action Mode UI")]
        [SerializeField] private GameObject _actionPanel; // Reload slider panel
        [SerializeField] private Slider _cooldownSlider;
        [SerializeField] private TextMeshProUGUI _cooldownStatusText;

        [Header("Active Plants UI")]
        [SerializeField] private Transform _activePlantsContainer;
        [SerializeField] private ActivePlantView _activePlantPrefab;
        
        [Header("Game Over UI")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverTitle; 
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        
        private IEconomyService _economyService;
        private IStaticDataProvider _staticData;
        private IPlantingService _plantingService;
        private IPlantTrackerService _plantTracker;
        private IInputService _inputService;
        private IYandexService _yandexService;
        private DiContainer _container;

        private readonly List<PlantCard> _cards = new();
        private IPossessablePlant _activePlant;
        private float _warningTimer;
        private Tween _arrowTween;
        private bool _isMobilePlatform;

        [Inject]
        public void Construct(
            IEconomyService economyService,
            IStaticDataProvider staticData,
            IPlantingService plantingService,
            IPlantTrackerService plantTracker,
            IInputService inputService,
            IYandexService yandexService,
            DiContainer container)
        {
            _economyService = economyService;
            _staticData = staticData;
            _plantingService = plantingService;
            _plantTracker = plantTracker;
            _inputService = inputService;
            _yandexService = yandexService;
            _container = container;
        }

        private void Start()
        {
            InitializeCards();
            SetupInputRegistration();
            LocalizeTexts();

            _gameOverPanel.SetActive(false);
            
            _economyService.OnSunChanged += UpdateSunDisplay;
            UpdateSunDisplay(_economyService.CurrentSun);
            
            _plantingService.OnPlantSelected += HandlePlantSelected;
            _plantTracker.OnListChanged += RebuildActivePlantsList;

            // Start in Planting Mode (No active plant)
            SetActivePlant(null);
        }

        private void OnDestroy()
        {
            _economyService.OnSunChanged -= UpdateSunDisplay;
            _plantingService.OnPlantSelected -= HandlePlantSelected;
            if (_plantTracker != null) _plantTracker.OnListChanged -= RebuildActivePlantsList;
            
            _arrowTween?.Kill();
            UnsubscribeActivePlant();
        }

        private void SetupInputRegistration()
        {
            _isMobilePlatform = YG.YG2.envir.isMobile || Application.isMobilePlatform;

            if (_isMobilePlatform)
            {
                _inputService.RegisterMobileControls(_mobileControls);
                // Initially hide joystick, we start in Planting Mode
                _mobileControls.SetVisible(false);
            }
            else
            {
                _mobileControls.SetVisible(false);
                _inputService.RegisterMobileControls(null);
            }
        }
        
        private void LocalizeTexts()
        {
            // Only dynamic texts handled here or static simple keys
        }

        private void Update()
        {
            UpdateCardsAvailability();
            UpdatePlantUI();
            
            if (_plantingGhostImage.gameObject.activeSelf)
            {
                _plantingGhostImage.transform.position = _inputService.GetPointerPosition();
            }
            
            if (_inputService is InputService concreteInput)
            {
                concreteInput.CheckMobileInput();
            }
        }

        public void BindButtons(Action onRestart, Action onMenu, Action onStartBattle, Action onSettings)
        {
            _restartButton.onClick.AddListener(() => onRestart?.Invoke());
            _menuButton.onClick.AddListener(() => onMenu?.Invoke());
            _exitToMenuButton.onClick.AddListener(() => onMenu?.Invoke());
            _settingsButton.onClick.AddListener(() => onSettings?.Invoke());
            
            // StartBattleButton is removed, logic is handled via possession
        }

        public void ShowGameOverPanel(bool isVictory)
        {
            _gameOverPanel.SetActive(true);
            _gameOverTitle.text = _yandexService.GetText(isVictory ? "VICTORY" : "GAME_OVER");
            
            // Hide all controls on Game Over
            _mobileControls.SetVisible(false);
            _gameplayPanel.SetActive(false);
            _actionPanel.SetActive(false);
            
            HideTutorialArrow();
            SetGhost(null);
            SetDimmed(false);
        }

        public void SetActivePlant(IPossessablePlant plant)
        {
            UnsubscribeActivePlant();
            _activePlant = plant;
            
            bool isPossessed = _activePlant != null;
            
            // Toggle Panels based on Mode
            if (isPossessed)
            {
                // Action Mode
                _gameplayPanel.SetActive(false); // Hide Cards
                ToggleActionPanel(true); // Show Reload Slider
                
                if (_isMobilePlatform) 
                    _mobileControls.SetVisible(true); // Show Joystick & Fire

                _activePlant.OnFireFailedCooldown += ShowCooldownWarning;
            }
            else
            {
                // Planting Mode
                _gameplayPanel.SetActive(true); // Show Cards
                ToggleActionPanel(false); // Hide Reload Slider
                
                if (_isMobilePlatform) 
                    _mobileControls.SetVisible(false); // Hide Joystick
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
            _cooldownStatusText.text = _yandexService.GetText("NOT_READY");
            _cooldownStatusText.color = Color.red;
            _warningTimer = 1.0f;
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
                _cooldownStatusText.text = _yandexService.GetText("READY"); 
                _cooldownStatusText.color = Color.green; 
            }
            else if (_warningTimer > 0) 
            { 
                _warningTimer -= Time.deltaTime; 
            }
            else 
            { 
                _cooldownStatusText.text = _yandexService.GetText("RELOADING"); 
                _cooldownStatusText.color = Color.white; 
            }
        }

        public void SetDimmed(bool isActive) => _dimmedPanel.SetActive(isActive);
        
        public void ShowTutorialStep1_Selection()
        {
            SetupArrow();
            _tutorialArrow.position = _tutorialOrigin1.position;
            _arrowTween = _tutorialArrow.DOMoveX(_tutorialArrow.position.x + 20f, _arrowAnimDuration).SetLoops(-1, LoopType.Yoyo);
        }

        public void ShowTutorialStep2_Placement()
        {
            SetupArrow();
            _tutorialArrow.position = _tutorialOrigin2.position;
            _tutorialArrow.rotation = Quaternion.Euler(0, 0, -90);
            _arrowTween = _tutorialArrow.DOMoveY(_tutorialArrow.position.y - 20f, _arrowAnimDuration).SetLoops(-1, LoopType.Yoyo);
        }

        public void ShowTutorialStep3_Possession()
        {
            if (!_tutorialArrow.gameObject.activeSelf) ShowTutorialStep2_Placement();
        }

        public void HideTutorialArrow()
        {
            _arrowTween?.Kill();
            if (_tutorialArrow != null) _tutorialArrow.gameObject.SetActive(false);
        }

        public void SetGhost(Sprite sprite)
        {
            if (_plantingGhostImage == null) return;
            if (sprite != null) { _plantingGhostImage.sprite = sprite; _plantingGhostImage.gameObject.SetActive(true); }
            else _plantingGhostImage.gameObject.SetActive(false);
        }

        private void SetupArrow()
        {
            _tutorialArrow.gameObject.SetActive(true);
            _arrowTween?.Kill();
            _tutorialArrow.rotation = Quaternion.identity;
        }

        private void UpdateCardsAvailability()
        {
            int currentSun = _economyService.CurrentSun;
            var plants = _staticData.GetAllPlants();
            for (int i = 0; i < _cards.Count && i < plants.Count; i++) _cards[i].SetAffordable(currentSun >= plants[i].cost);
        }

        private void InitializeCards()
        {
            foreach (Transform child in _cardsContainer) Destroy(child.gameObject);
            _cards.Clear();
            var plants = _staticData.GetAllPlants();
            foreach (var plantData in plants)
            {
                var cardInstance = _container.InstantiatePrefabForComponent<PlantCard>(_cardPrefab, _cardsContainer);
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
                var view = _container.InstantiatePrefabForComponent<ActivePlantView>(_activePlantPrefab, _activePlantsContainer);
                PlantType type = PlantType.None;
                if (plant is CannonController) type = PlantType.CoconutCannon;
                else if (plant is PeashooterController) type = PlantType.Peashooter;
                
                view.Initialize(plant, i, type, (keyIndex) => 
                {
                    _inputService.TriggerHotbar(keyIndex);
                });
                
                view.SetSelected(_activePlant == plant);
            }
        }

        private void OnCardClicked(PlantData data) => _plantingService.SelectPlant(data.type);
        private void HandlePlantSelected(PlantType type)
        {
            foreach (var card in _cards)
            {
                card.CheckSelection(type);
            }
        }
        private void UpdateSunDisplay(int amount) => _sunText.text = $"{amount}";
        public void SetGameplayVisibility(bool isVisible) => _gameplayPanel.SetActive(isVisible);
        private void ToggleActionPanel(bool isActive) => _actionPanel.SetActive(isActive);
        public void SetStartButtonVisible(bool isVisible) { } // Removed logic, keeping method stub to prevent breaks if called
    }
}