using Core.BaseStates;

namespace Infrastructure.Factories.States
{
    public interface IStateFactory
    {
        IState CreateState<TState>() where TState : IState;
    }
}