using System;
using Infrastructure.Services.Economy;
using Infrastructure.Services.FPS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.HUD
{
    /// <summary>
    /// Controls HUD, including Sun economy, FPS, and Wave controls.
    /// </summary>
    public class HudWindow : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _fpsText;
        [SerializeField] private TextMeshProUGUI _sunText;
        [SerializeField] private TextMeshProUGUI _waveText;
        
        [Header("Controls")]
        [SerializeField] private Button _startWaveButton;
        [SerializeField] private GameObject _controlsPanel;

        [SerializeField] private float _updateInterval = 0.2f;

        public event Action OnStartWaveClicked;

        private IFPSService _fpsService;
        private IEconomyService _economyService;
        private float _lastUpdateTimer;

        [Inject]
        public void Construct(IFPSService fpsService, IEconomyService economyService)
        {
            _fpsService = fpsService;
            _economyService = economyService;
        }

        private void Start()
        {
            _startWaveButton.onClick.AddListener(() => OnStartWaveClicked?.Invoke());
            _economyService.OnSunChanged += UpdateSun;
            UpdateSun(_economyService.CurrentSun);
        }

        private void OnDestroy()
        {
            _startWaveButton.onClick.RemoveAllListeners();
            _economyService.OnSunChanged -= UpdateSun;
        }

        private void Update()
        {
            _lastUpdateTimer += Time.deltaTime;

            if (_lastUpdateTimer >= _updateInterval)
            {
                _fpsText.text = $"FPS: {_fpsService.CurrentFps:F0}";
                _lastUpdateTimer = 0f;
            }
        }

        private void UpdateSun(int amount)
        {
            _sunText.text = $"{amount}";
        }

        public void SetWaveInfo(int waveNumber)
        {
            _waveText.text = $"Wave: {waveNumber}";
        }

        public void SetStartButtonVisible(bool isVisible)
        {
            _startWaveButton.gameObject.SetActive(isVisible);
        }
        
        public void ToggleControls(bool isVisible)
        {
            _controlsPanel.SetActive(isVisible);
        }
    }
}