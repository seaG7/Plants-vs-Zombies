using Cysharp.Threading.Tasks;
using Features.Enemy;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Infrastructure.Factories.Enemies
{
    public interface IEnemyFactory
    {
        UniTask<ZombieController> CreateZombie(AssetReference assetRef, Vector3 position);
    }
}