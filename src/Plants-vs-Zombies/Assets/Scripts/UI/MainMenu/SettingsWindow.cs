using Infrastructure.Services;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Window;
using Infrastructure.Services.Yandex;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.MainMenu
{
    /// <summary>
    /// Controls settings panel, audio sliders and localization.
    /// </summary>
    public class SettingsWindow : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private TMPro.TextMeshProUGUI _titleText;
        [SerializeField] private TMPro.TextMeshProUGUI _sfxLabel;
        [SerializeField] private TMPro.TextMeshProUGUI _musicLabel;

        private IWindowService _windowService;
        private IAudioService _audioService;
        private IYandexService _yandexService;

        [Inject]
        public void Construct(
            IWindowService windowService, 
            IAudioService audioService,
            IYandexService yandexService)
        {
            _windowService = windowService;
            _audioService = audioService;
            _yandexService = yandexService;
        }

        private void Start()
        {
            Localize();
            InitializeState();
            
            _closeButton.onClick.AddListener(OnCloseClicked);
            _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        private void Localize()
        {
            if (_titleText) _titleText.text = _yandexService.GetText("SETTINGS");
            if (_sfxLabel) _sfxLabel.text = _yandexService.GetText("SFX");
            if (_musicLabel) _musicLabel.text = _yandexService.GetText("MUSIC");
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnCloseClicked);
            _sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        }

        private void InitializeState()
        {
            _sfxSlider.value = _audioService.SfxVolume;
            _musicSlider.value = _audioService.MusicVolume;
        }

        private void OnSfxChanged(float value)
        {
            _audioService.SetSfxVolume(value);
        }

        private void OnMusicChanged(float value)
        {
            _audioService.SetMusicVolume(value);
        }

        private void OnCloseClicked()
        {
            _windowService.Close(WindowID.Settings);
        }
    }
}