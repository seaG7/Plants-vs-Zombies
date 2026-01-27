using System.Collections.Generic;
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
        
        private readonly List<GameObject> _trackedObjects = new();

        public GameObjectFactory(DiContainer globalContainer, IAssetsAddressablesProvider assetsProvider)
        {
            _globalContainer = globalContainer;
            _assetsProvider = assetsProvider;
        }
        
        public async UniTask<GameObject> InstantiateAsync(string path, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null, DiContainer container = null) =>
            Register(InstantiateAsync(await _assetsProvider.GetAsset<GameObject>(path), position, rotation, parent, container));

        public async UniTask<GameObject> InstantiateAsync(AssetReference path, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null) =>
            Register(InstantiateAsync(await _assetsProvider.GetAsset<GameObject>(path), position, rotation, parent));

        public async UniTask<T> InstantiateAndGetComponent<T>(string path, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null) where T : class =>
            (await InstantiateAsync(path, position, rotation, parent)).GetComponent<T>();

        public async UniTask<T> InstantiateAndGetComponent<T>(AssetReference path, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null) where T : class =>
            (await InstantiateAsync(path, position, rotation, parent)).GetComponent<T>();

        public void Destroy(GameObject gameObject)
        {
            if (gameObject != null)
            {
                _trackedObjects.Remove(gameObject);
                Object.Destroy(gameObject);
            }
        }

        public void Cleanup()
        {
            foreach (var obj in _trackedObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            _trackedObjects.Clear();
        }

        private GameObject InstantiateAsync(GameObject prefab = null, Vector3? pos = null, Quaternion? rot = null,
            Transform parent = null, DiContainer container = null)
        {
            var containerToUse = container ?? _globalContainer;
            var obj = containerToUse.InstantiatePrefab(prefab, pos ?? Vector3.zero, rot ?? Quaternion.identity, parentTransform: null);
            
            if (parent != null) obj.transform.SetParent(parent);

            return obj;
        }

        private GameObject Register(GameObject obj)
        {
            if (obj != null) _trackedObjects.Add(obj);
            return obj;
        }
    }
}