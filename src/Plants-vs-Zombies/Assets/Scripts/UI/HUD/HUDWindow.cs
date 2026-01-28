using System;
using System.Collections.Generic;
using Core.Interfaces;
using Data.Configs;
using Data.Enums;
using DG.Tweening;
using Features.Plants;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services.Economy;
using Infrastructure.Services.FPS;
using Infrastructure.Services.Input;
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
        
        [Header("Controls")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitToMenuButton;
        
        [Header("Planting UI")]
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private PlantCard _cardPrefab;
        [SerializeField] private GameObject _gameplayPanel;
        [SerializeField] private Image _plantingGhostImage; 

        [Header("Tutorial UI")]
        [SerializeField] private GameObject _dimmedPanel; 
        [SerializeField] private RectTransform _tutorialArrow;
        [SerializeField] private RectTransform _tutorialOrigin1; // Выбор карты
        [SerializeField] private RectTransform _tutorialOrigin2; // Поле (Grid)
        
        [SerializeField] private float _arrowAnimDuration = 0.6f;

        [Header("Action Mode UI")]
        [SerializeField] private GameObject _actionPanel;
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
        
        [Header("Battle Controls")]
        [SerializeField] private Button _startBattleButton;

        // ... (Services and fields) ...
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
        private IInputService _inputService;
        private DiContainer _container;

        private readonly List<PlantCard> _cards = new();
        private float _lastUpdateTimer;
        private IPossessablePlant _activePlant;
        private float _warningTimer;
        private Tween _arrowTween;

        [Inject]
        public void Construct(
            IFPSService fpsService, 
            IEconomyService economyService,
            IStaticDataProvider staticData,
            IPlantingService plantingService,
            IPlantTrackerService plantTracker,
            IInputService inputService,
            DiContainer container)
        {
            _fpsService = fpsService;
            _economyService = economyService;
            _staticData = staticData;
            _plantingService = plantingService;
            _plantTracker = plantTracker;
            _inputService = inputService;
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
            
            _arrowTween?.Kill();
            UnsubscribeActivePlant();
        }

        // --- Tutorial Public Methods ---

        public void SetDimmed(bool isActive)
        {
            if (_dimmedPanel != null)
                _dimmedPanel.SetActive(isActive);
        }

        public void ShowTutorialStep1_Selection()
        {
            if (_tutorialOrigin1 == null) return;
            
            SetupArrow();
            
            _tutorialArrow.position = _tutorialOrigin1.position;
            _tutorialArrow.rotation = Quaternion.Euler(0, 0, 0);

            float startX = _tutorialArrow.position.x;
            _arrowTween = _tutorialArrow.DOMoveX(startX + 20f, _arrowAnimDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        public void ShowTutorialStep2_Placement()
        {
            if (_tutorialOrigin2 == null) return;
            SetupArrow();
            AnimateArrowAtOrigin2();
        }

        // ШАГ 3: Оставляем стрелку там же, где она была (Origin2)
        public void ShowTutorialStep3_Possession()
        {
            if (_tutorialOrigin2 == null) return;
            // Убеждаемся, что стрелка активна и на месте
            if (!_tutorialArrow.gameObject.activeSelf)
            {
                SetupArrow();
                AnimateArrowAtOrigin2();
            }
            // Можно добавить пульсацию, если хочется отличий
        }

        private void AnimateArrowAtOrigin2()
        {
            _tutorialArrow.position = _tutorialOrigin2.position;
            _tutorialArrow.rotation = Quaternion.Euler(0, 0, -90);

            float startY = _tutorialArrow.position.y;
            _arrowTween = _tutorialArrow.DOMoveY(startY - 20f, _arrowAnimDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        public void HideTutorialArrow()
        {
            if (_tutorialArrow != null)
            {
                _arrowTween?.Kill();
                _tutorialArrow.gameObject.SetActive(false);
            }
        }

        public void SetGhost(Sprite sprite)
        {
            if (_plantingGhostImage == null) return;

            if (sprite != null)
            {
                _plantingGhostImage.sprite = sprite;
                _plantingGhostImage.gameObject.SetActive(true);
            }
            else
            {
                _plantingGhostImage.gameObject.SetActive(false);
            }
        }

        private void SetupArrow()
        {
            if (_tutorialArrow == null) return;
            _tutorialArrow.gameObject.SetActive(true);
            _arrowTween?.Kill();
            _tutorialArrow.localScale = Vector3.one;
        }

        private void Update()
        {
            UpdateFPS();
            UpdateCardsAvailability();
            UpdatePlantUI();
            
            if (_plantingGhostImage.gameObject.activeSelf)
            {
                _plantingGhostImage.transform.position = _inputService.GetPointerPosition();
            }
            // Логика 3D трекинга полностью убрана
        }
        
        // ... (BindButtons, ShowGameOverPanel, SetActivePlant...)
        public void BindButtons(Action onRestart, Action onMenu, Action onStartBattle, Action onSettings)
        {
            _restartButton.onClick.AddListener(() => onRestart?.Invoke());
            _menuButton.onClick.AddListener(() => onMenu?.Invoke());
            _exitToMenuButton.onClick.AddListener(() => onMenu?.Invoke());
            _settingsButton.onClick.AddListener(() => onSettings?.Invoke());
            _startBattleButton.onClick.AddListener(() => onStartBattle?.Invoke());
        }

        public void ShowGameOverPanel(bool isVictory)
        {
            _gameOverPanel.SetActive(true);
            _gameOverTitle.text = isVictory ? "VICTORY!" : "GAME OVER";
            _settingsButton.gameObject.SetActive(false);
            _exitToMenuButton.gameObject.SetActive(false);
            
            HideTutorialArrow();
            SetGhost(null);
            SetDimmed(false);
        }

        public void SetActivePlant(IPossessablePlant plant)
        {
            UnsubscribeActivePlant();
            _activePlant = plant;
            if (_activePlant != null) { ToggleActionPanel(true); _activePlant.OnFireFailedCooldown += ShowCooldownWarning; }
            else { ToggleActionPanel(false); }
            RebuildActivePlantsList();
        }

        private void UnsubscribeActivePlant()
        {
            if (_activePlant != null) { _activePlant.OnFireFailedCooldown -= ShowCooldownWarning; _activePlant = null; }
        }
        
        private void ShowCooldownWarning()
        {
            _cooldownStatusText.text = _textNotReadyWarning;
            _cooldownStatusText.color = _colorNotReady;
            _warningTimer = 1.0f;
        }

        private void UpdatePlantUI()
        {
            if (_activePlant == null || !_actionPanel.activeSelf) return;
            float progress = _activePlant.GetReloadProgress();
            _cooldownSlider.value = progress;
            bool isReady = progress >= 0.99f;
            if (isReady) { _warningTimer = 0f; _cooldownStatusText.text = _textReady; _cooldownStatusText.color = _colorReady; }
            else if (_warningTimer > 0) { _warningTimer -= Time.deltaTime; }
            else { _cooldownStatusText.text = _textReloading; _cooldownStatusText.color = Color.white; }
        }

        private void UpdateFPS()
        {
            _lastUpdateTimer += Time.deltaTime;
            if (_lastUpdateTimer >= 0.5f) { _fpsText.text = $"{_fpsService.CurrentFps:F0}"; _lastUpdateTimer = 0f; }
        }
        
        private void UpdateCardsAvailability()
        {
            int currentSun = _economyService.CurrentSun;
            var plants = _staticData.GetAllPlants();
            for (int i = 0; i < _cards.Count && i < plants.Count; i++) { _cards[i].SetAffordable(currentSun >= plants[i].cost); }
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
                var view = _container.InstantiatePrefabForComponent<ActivePlantView>(_activePlantPrefab, _activePlantsContainer);
                PlantType type = PlantType.None;
                if (plant is CannonController) type = PlantType.CoconutCannon;
                else if (plant is PeashooterController) type = PlantType.Peashooter;
                view.Initialize(plant, i, type);
                view.SetSelected(_activePlant == plant);
            }
        }

        private void OnCardClicked(PlantData data) => _plantingService.SelectPlant(data.type);
        private void HandlePlantSelected(PlantType type) { }
        private void UpdateSunDisplay(int amount) => _sunText.text = $"{amount}";
        public void SetGameplayVisibility(bool isVisible) => _gameplayPanel.SetActive(isVisible);
        private void ToggleActionPanel(bool isActive) { if (_actionPanel != null) _actionPanel.SetActive(isActive); }
        public void SetStartButtonVisible(bool isVisible) { if (_startBattleButton != null) _startBattleButton.gameObject.SetActive(isVisible); }
    }
}