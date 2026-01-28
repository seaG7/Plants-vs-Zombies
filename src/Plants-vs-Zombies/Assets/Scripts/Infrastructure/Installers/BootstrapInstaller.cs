using Core.BaseStates;
using Infrastructure.Factories.Enemies;
using Infrastructure.Factories.Objects;
using Infrastructure.Factories.States;
using Infrastructure.Factories.UI;
using Infrastructure.Providers.AssetsAddressables;
using Infrastructure.Providers.Context;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Economy;
using Infrastructure.Services.FPS;
using Infrastructure.Services.Grid;
using Infrastructure.Services.Input;
using Infrastructure.Services.Lanes;
using Infrastructure.Services.PhysicsCalculation;
using Infrastructure.Services.Planting;
using Infrastructure.Services.Scene;
using Infrastructure.Services.UI;
using Infrastructure.Services.Waves;
using Infrastructure.Services.Window;
using Zenject;

namespace Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindBootstrap();
            BindProviders();
            BindServices();
            BindFactories();
        }

        private void BindBootstrap()
        {
            Container.Bind<BootstrapLoader>().AsSingle().NonLazy();
            Container.Bind<IInitializable>().To<BootstrapLoader>().FromResolve();
        }

        private void BindProviders()
        {
            Container.Bind<IAssetsAddressablesProvider>().To<AssetsAddressablesProvider>().AsSingle();
            Container.Bind<ILevelProvider>().To<LevelProvider>().AsSingle();
            Container.Bind<IStaticDataProvider>().To<StaticDataProvider>().AsSingle();
        }

        private void BindServices()
        {
            Container.Bind<StateMachine>().AsSingle();
            Container.Bind<ISceneLoaderService>().To<SceneLoaderService>().AsSingle();
            Container.Bind<IWindowService>().To<WindowService>().AsSingle();
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
            
            Container.Bind<IAerodynamicsCalculationService>().To<AerodynamicsCalculationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<FPSService>().AsSingle();

            Container.Bind<ILaneService>().To<LaneService>().AsSingle();
            Container.Bind<IGridService>().To<GridService>().AsSingle();
            Container.BindInterfacesAndSelfTo<WaveService>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<EconomyService>().AsSingle();
            Container.Bind<ICameraService>().To<CameraService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlantingService>().AsSingle();
            Container.Bind<IPlantTrackerService>().To<PlantTrackerService>().AsSingle();
            Container.Bind<IAudioService>().To<AudioService>().AsSingle();
        }

        private void BindFactories()
        {
            Container.Bind<IStateFactory>().To<StateFactory>().AsSingle();
            Container.Bind<IUIFactory>().To<UIFactory>().AsSingle();
            Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
        }
    }
}