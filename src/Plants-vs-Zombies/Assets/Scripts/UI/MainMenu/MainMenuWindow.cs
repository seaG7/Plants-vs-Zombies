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
    /// Controls the main menu interaction logic.
    /// </summary>
    public class MainMenuWindow : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;

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
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        private void OnStartClicked()
        {
            _stateMachine.ChangeState<GameLoadState>();
        }

        private void OnSettingsClicked()
        {
            _windowService.Open(WindowID.Settings);
        }
    }
}