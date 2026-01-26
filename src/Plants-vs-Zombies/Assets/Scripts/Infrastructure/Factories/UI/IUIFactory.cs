using System.Threading.Tasks;
using Infrastructure.Services;
using UnityEngine;
using Zenject;

namespace Infrastructure.Factories.UI
{
    public interface IUIFactory
    {
        Task<GameObject> CreateScreen(string assetAddress, WindowID windowId);
        T GetScreenComponent<T>(WindowID windowId) where T : Component;
        void DestroyScreen(WindowID windowId);
        bool Exists(WindowID windowId);
    }
}