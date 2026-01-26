using Core.BaseStates;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.Waves;
using Infrastructure.Services.Window;

namespace Core.States
{
    /// <summary>
    /// State responsible for the active gameplay loop.
    /// </summary>
    public class GameplayState : IState, IEnterable, IExitable
    {
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private readonly IWaveService _waveService;

        public GameplayState(IWindowService windowService, IInputService inputService, IWaveService waveService)
        {
            _windowService = windowService;
            _inputService = inputService;
            _waveService = waveService;
        }

        public async void Enter()
        {
            await _windowService.Open(WindowID.HUD);
            
            _inputService.Enable();
            
            _waveService.StartLevel();
        }

        public void Exit()
        {
            _windowService.Close(WindowID.HUD);
            _inputService.Disable();
        }
    }
}