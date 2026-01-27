using Infrastructure.Services;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Window;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.MainMenu
{
    /// <summary>
    /// Controls settings panel, audio sliders and closing logic.
    /// </summary>
    public class SettingsWindow : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _musicSlider;

        private IWindowService _windowService;
        private IAudioService _audioService;

        [Inject]
        public void Construct(IWindowService windowService, IAudioService audioService)
        {
            _windowService = windowService;
            _audioService = audioService;
        }

        private void Start()
        {
            InitializeState();
            
            _closeButton.onClick.AddListener(OnCloseClicked);
            _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);
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