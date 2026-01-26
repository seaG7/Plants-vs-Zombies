using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Infrastructure.Factories.Objects
{
    public interface IGameObjectFactory
    {
        UniTask<GameObject> InstantiateAsync(
            string path,
            Vector3? position = null,
            Quaternion? rotation = null,
            Transform parent = null,
            DiContainer container = null
        );

        UniTask<GameObject> InstantiateAsync(
            AssetReference path,
            Vector3? position = null,
            Quaternion? rotation = null,
            Transform parent = null
        );

        UniTask<T> InstantiateAndGetComponent<T>(
            string path,
            Vector3? position = null,
            Quaternion? rotation = null,
            Transform parent = null
        ) where T : class;

        UniTask<T> InstantiateAndGetComponent<T>(
            AssetReference path,
            Vector3? position = null,
            Quaternion? rotation = null,
            Transform parent = null
        ) where T : class;

        void Destroy(GameObject gameObject);
    }
}