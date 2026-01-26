using Core.BaseStates;
using Core.States;
using Infrastructure.Providers.StaticData;
using Zenject;

namespace Infrastructure.Installers
{
    public class BootstrapLoader : IInitializable
    {
        private readonly StateMachine _stateMachine;
        private readonly IStaticDataProvider _provider;

        public BootstrapLoader(StateMachine stateMachine, IStaticDataProvider staticDataProvider)
        {
            _stateMachine = stateMachine;
            _provider = staticDataProvider;
        }
    
        public void Initialize()
        {
            _provider.Load();
            
            _stateMachine.ChangeState<BootstrapState>();
        }
    }
}