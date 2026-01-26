using Cysharp.Threading.Tasks;
using Infrastructure.Providers.AssetsAddressables;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Infrastructure.Factories.Objects
{
    public class GameObjectFactory : IGameObjectFactory
    {
        private readonly DiContainer _globalContainer;
        private readonly IAssetsAddressablesProvider _assetsProvider;

        public GameObjectFactory(DiContainer globalContainer, IAssetsAddressablesProvider assetsProvider)
        {
            _globalContainer = globalContainer;
            _assetsProvider = assetsProvider;
        }
        
        public async UniTask<GameObject> InstantiateAsync(string path, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null, DiContainer container = null) =>
            InstantiateAsync(await _assetsProvider.GetAsset<GameObject>(path), position, rotation, parent, container);

        public async UniTask<GameObject> InstantiateAsync(AssetReference path, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null) =>
            InstantiateAsync(await _assetsProvider.GetAsset<GameObject>(path), position, rotation, parent);

        public async UniTask<T> InstantiateAndGetComponent<T>(string path, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null) where T : class =>
            (await InstantiateAsync(path, position, rotation, parent)).GetComponent<T>();

        public async UniTask<T> InstantiateAndGetComponent<T>(AssetReference path, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null) where T : class =>
            (await InstantiateAsync(path, position, rotation, parent)).GetComponent<T>();

        public void Destroy(GameObject gameObject) => Object.Destroy(gameObject);

        private GameObject InstantiateAsync(GameObject prefab = null, Vector3? pos = null, Quaternion? rot = null,
            Transform parent = null, DiContainer container = null)
        {
            var containerToUse = container ?? _globalContainer;
            var obj = containerToUse.InstantiatePrefab(prefab, pos ?? Vector3.zero, rot ?? Quaternion.identity, parentTransform: null);
            if (container != null) obj.transform.SetParent(parent);

            return obj;
        }
    }
}