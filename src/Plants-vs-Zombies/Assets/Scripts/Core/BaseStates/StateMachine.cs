using System.Threading;
using Infrastructure.Factories.States;

namespace Core.BaseStates
{
    public class StateMachine
    {
        private readonly IStateFactory _stateFactory;
        private IState _currentState;
        private CancellationTokenSource _tickCancellationTokenSource;

        public StateMachine(IStateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public void ChangeState<TState>() where TState : IState
        {
            ExitCurrentState();
            _currentState = _stateFactory.CreateState<TState>();
            EnterCurrentState();
        }

        public void ChangeState<TState, T0>(T0 arg) where TState : IState
        {
            ExitCurrentState();
            _currentState = _stateFactory.CreateState<TState>();
            EnterCurrentState(arg);
        }

        private void ExitCurrentState()
        {
            if (_currentState is IExitable exitable)
                exitable.Exit();

            _tickCancellationTokenSource?.Cancel();
        }

        private void EnterCurrentState()
        {
            if (_currentState is IEnterable enterable)
                enterable.Enter();
        }
        
        private void EnterCurrentState<T0>(T0 arg)
        {
            if (_currentState is IEnterableWithArg<T0> enterable)
                enterable.Enter(arg);
        }
    }
}