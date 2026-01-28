using Core.BaseStates;
using Core.States;
using Infrastructure.Services;
using Infrastructure.Services.Window;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.MainMenu
{
    /// <summary>
    /// Controls main menu interactions: Start, Settings, Exit.
    /// </summary>
    public class MainMenuWindow : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        private StateMachine _stateMachine;
        private IWindowService _windowService;

        [Inject]
        public void Construct(StateMachine stateMachine, IWindowService windowService)
        {
            _stateMachine = stateMachine;
            _windowService = windowService;
        }

        private void Start()
        {
            _startButton.onClick.AddListener(OnStartClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _exitButton.onClick.RemoveListener(OnExitClicked);
        }

        private void OnStartClicked()
        {
            _stateMachine.ChangeState<GameLoadState>();
        }

        private void OnSettingsClicked()
        {
            _windowService.Open(WindowID.Settings);
        }

        private void OnExitClicked()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}