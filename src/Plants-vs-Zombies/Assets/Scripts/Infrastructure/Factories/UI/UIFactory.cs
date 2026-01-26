using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Factories.Objects;
using Infrastructure.Services;
using UnityEngine;
using Zenject;

namespace Infrastructure.Factories.UI
{
    public class UIFactory : IUIFactory
    {
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly Dictionary<WindowID, GameObject> _screenInstances = new();

        public UIFactory(
            IGameObjectFactory gameObjectFactory
        )
        {
            _gameObjectFactory = gameObjectFactory;
        }

        public async Task<GameObject> CreateScreen(string assetAddress, WindowID windowId)
        {
            if (_screenInstances.ContainsKey(windowId))
            {
                Debug.LogWarning($"Screen with WindowID {windowId} already exists.. " +
                                 $"Swapping screens.");

                DestroyScreen(windowId);
            }

            var instance = await _gameObjectFactory.InstantiateAsync(assetAddress);

            if (_screenInstances.TryAdd(windowId, instance))
            {
                return instance;
            }

            Object.Destroy(instance);
            return null;
        }

        public T GetScreenComponent<T>(WindowID windowId) where T : Component
        {
            if (_screenInstances.TryGetValue(windowId, out var screenObject))
            {
                var screenComponent = screenObject.GetComponent<T>();
                if (screenComponent != null)
                {
                    return screenComponent;
                }

                Debug.LogError($"Component of screen by type {typeof(T)} not found.");
                return null;
            }

            Debug.LogError($"Screen with WindowID {windowId} not found.");
            return null;
        }

        public void DestroyScreen(WindowID windowId)
        {
            if (!_screenInstances.Remove(windowId, out var screenObject))
            {
                Debug.LogWarning($"Screen with WindowID {windowId} not found");
                return;
            }

            _gameObjectFactory.Destroy(screenObject);
        }

        public bool Exists(WindowID windowId) => _screenInstances.ContainsKey(windowId);
    }
}