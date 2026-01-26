using Zenject;

namespace Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BootstrapLoader>().AsSingle().NonLazy();

            Container.Bind<IInitializable>().To<BootstrapLoader>().FromResolve();
        }
    }
}