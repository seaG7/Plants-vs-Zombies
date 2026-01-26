using Core;
using Core.BaseStates;
using Core.States;
using Zenject;

namespace Infrastructure
{
    public class BootstrapLoader : IInitializable
    {
        private readonly StateMachine _stateMachine;

        public BootstrapLoader(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }
    
        public void Initialize()
        {
            _stateMachine.ChangeState<BootstrapState>();
        }
    }
}