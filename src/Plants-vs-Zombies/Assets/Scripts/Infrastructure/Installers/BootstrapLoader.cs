using Core.BaseStates;
using Core.States;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services;
using Infrastructure.Services.Window;
using Zenject;

namespace Infrastructure.Installers
{
    /// <summary>
    /// Точка входа в логику приложения. Запускает экран загрузки и инициализирует данные.
    /// </summary>
    public class BootstrapLoader : IInitializable
    {
        private readonly StateMachine _stateMachine;
        private readonly IStaticDataProvider _provider;
        private readonly IWindowService _windowService;

        public BootstrapLoader(StateMachine stateMachine, IStaticDataProvider staticDataProvider, IWindowService windowService)
        {
            _stateMachine = stateMachine;
            _provider = staticDataProvider;
            _windowService = windowService;
        }
    
        public async void Initialize()
        {
            _provider.Load();
            
            _stateMachine.ChangeState<BootstrapState>();
        }
    }
}