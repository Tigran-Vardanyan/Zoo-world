using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Animals;
using ZooWorld.FoodChain;
using ZooWorld.Spawning;
using ZooWorld.Statistics;
using ZooWorld.UI;
using ZooWorld.World;

namespace ZooWorld.Installers
{
    /// <summary>
    /// Composition root of the Zoo scene: the only place that knows which implementation
    /// stands behind each interface. All tuning values are edited on this component in the Inspector.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("Scene")]
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _animalsRoot;
        [SerializeField] private Transform _obstaclesRoot;

        [Header("Animals")]
        [SerializeField] private AnimalCatalog _catalog;
        [SerializeField] private SpawnerSettings _spawnerSettings = new();

        [Header("World")]
        [SerializeField] private PlayAreaSettings _playAreaSettings = new();
        [SerializeField] private FreeSpotSettings _freeSpotSettings = new();
        [SerializeField] private ObstacleSettings _obstacleSettings = new();
        [SerializeField] private FoodChainSettings _foodChainSettings = new();

        [Header("UI")]
        [SerializeField] private DeathCounterView _deathCounterView;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterWorld(builder);
            RegisterFoodChain(builder);
            RegisterSpawning(builder);
            RegisterUI(builder);
        }

        private void RegisterWorld(IContainerBuilder builder)
        {
            builder.RegisterComponent(_camera);
            builder.RegisterInstance(_playAreaSettings);
            builder.Register<IPlayArea, CameraPlayArea>(Lifetime.Singleton);
            builder.RegisterInstance(_freeSpotSettings);
            builder.Register<IFreeSpotFinder, PhysicsFreeSpotFinder>(Lifetime.Singleton);
        }

        private void RegisterFoodChain(IContainerBuilder builder)
        {
            builder.RegisterInstance(_foodChainSettings);
            builder.Register<IPredatorDuel, RandomPredatorDuel>(Lifetime.Singleton);
            builder.Register<IEncounterRule, DietEncounterRule>(Lifetime.Singleton);
            builder.Register<IRepelStrategy, ImpulseRepel>(Lifetime.Singleton);
            builder.Register<FoodChainService>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterSpawning(IContainerBuilder builder)
        {
            builder.RegisterInstance(_obstacleSettings);
            builder.RegisterEntryPoint<ObstacleSpawner>().WithParameter(_obstaclesRoot).AsSelf();

            builder.Register<AnimalRegistry>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterInstance(_catalog);
            builder.RegisterInstance(_spawnerSettings);
            builder.Register<AnimalPool>(Lifetime.Singleton)
                .WithParameter(_animalsRoot)
                .AsImplementedInterfaces();
            builder.RegisterEntryPoint<AnimalSpawner>().As<IAnimalSpawner>();
        }

        private void RegisterUI(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<DeathStatistics>().As<IDeathStatistics>();
            builder.RegisterComponent(_deathCounterView);
            builder.RegisterEntryPoint<DeathCounterPresenter>();
        }
    }
}
