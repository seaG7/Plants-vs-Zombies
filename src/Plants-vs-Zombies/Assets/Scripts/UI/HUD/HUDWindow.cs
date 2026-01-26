using Infrastructure.Services.FPS;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI.HUD
{
    /// <summary>
    /// Controls the Heads-Up Display, updating FPS and other realtime stats.
    /// </summary>
    public class HudWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _fpsText;
        [SerializeField] private float _updateInterval = 0.2f;

        private IFPSService _fpsService;
        private float _lastUpdateTimer;

        [Inject]
        public void Construct(IFPSService fpsService)
        {
            _fpsService = fpsService;
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
    }
}