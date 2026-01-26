namespace Core.BaseStates
{
    public interface IEnterableWithArg<in T0>
    {
        void Enter(T0 arg);
    }
}