using Core.BaseStates;
using Infrastructure.Factories.Objects;
using Infrastructure.Factories.States;
using Infrastructure.Factories.UI;
using Infrastructure.Providers.AssetsAddressables;
using Infrastructure.Services.Input;
using Infrastructure.Services.Physics;
using Infrastructure.Services.Scene;
using Infrastructure.Services.UI;
using Infrastructure.Services.Window;
using Zenject;

namespace Infrastructure.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindServices();
            BindProviders();
            BindFactories();
        }

        private void BindServices()
        {
            Container.Bind<StateMachine>().AsSingle();
            Container.Bind<ISceneLoaderService>().To<SceneLoaderService>().AsSingle();
            Container.Bind<IWindowService>().To<WindowService>().AsSingle();
            Container.Bind<IInputService>().To<InputService>().AsSingle();
            Container.Bind<IAerodynamicsCalculationService>().To<AerodynamicsCalculationService>().AsSingle();
        }

        private void BindProviders()
        {
            Container.Bind<IAssetsAddressablesProvider>().To<AssetsAddressablesProvider>().AsSingle();
        }

        private void BindFactories()
        {
            Container.Bind<IStateFactory>().To<StateFactory>().AsSingle();
            Container.Bind<IUIFactory>().To<UIFactory>().AsSingle();
            Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsSingle();
        }
    }
}