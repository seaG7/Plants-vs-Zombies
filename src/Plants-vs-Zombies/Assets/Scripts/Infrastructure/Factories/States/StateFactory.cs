using Core.BaseStates;
using Zenject;

namespace Infrastructure.Factories.States
{
    public class StateFactory : IStateFactory
    {
        private readonly DiContainer _container;

        public StateFactory(DiContainer container)
        {
            _container = container;
        }

        public IState CreateState<TState>() where TState : IState =>
            _container.Instantiate<TState>();
    }
}