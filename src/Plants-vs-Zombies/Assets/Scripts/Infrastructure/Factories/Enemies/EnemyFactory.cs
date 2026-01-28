using System;
using Cysharp.Threading.Tasks;
using Data.Configs;
using Features.Enemy;
using Infrastructure.Factories.Objects;
using Infrastructure.Providers.Context;
using UnityEngine;

namespace Infrastructure.Factories.Enemies
{
    public class EnemyFactory : IEnemyFactory
    {
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly ILevelProvider _levelProvider;
        
        public event Action<ZombieController> OnZombieCreated;

        public EnemyFactory(IGameObjectFactory gameObjectFactory, ILevelProvider levelProvider)
        {
            _gameObjectFactory = gameObjectFactory;
            _levelProvider = levelProvider;
        }

        public async UniTask<ZombieController> CreateZombie(EnemyData data, Vector3 position)
        {
            GameObject obj = await _gameObjectFactory.InstantiateAsync(data.prefabReference, position, Quaternion.LookRotation(Vector3.back));
            
            var zombie = obj.GetComponent<ZombieController>();
            zombie.Initialize(data);
            
            _levelProvider.CurrentLevel.RegisterEnemy(zombie);
            
            zombie.OnDeath += () => 
            {
                _levelProvider.CurrentLevel?.UnregisterEnemy(zombie);
            };
            
            OnZombieCreated?.Invoke(zombie);
            
            return zombie;
        }
    }
}