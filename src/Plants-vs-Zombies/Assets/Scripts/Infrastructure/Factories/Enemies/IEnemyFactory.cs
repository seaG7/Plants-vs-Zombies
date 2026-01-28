using System;
using Cysharp.Threading.Tasks;
using Data.Configs;
using Features.Enemy;
using UnityEngine;

namespace Infrastructure.Factories.Enemies
{
    public interface IEnemyFactory
    {
        UniTask<ZombieController> CreateZombie(EnemyData data, Vector3 position);
        event Action<ZombieController> OnZombieCreated;
    }
}