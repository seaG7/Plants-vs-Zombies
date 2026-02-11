// ===== UI/Loading/LoadingWindow.cs =====

using Infrastructure.Services.Window;
using UnityEngine;
using Zenject;

namespace UI
{
    /// <summary>
    /// Preloaded loading screen that registers itself to the global WindowService.
    /// </summary>
    public class LoadingWindow : MonoBehaviour
    {
        private IWindowService _windowService;

        [Inject]
        public void Construct(IWindowService windowService)
        {
            _windowService = windowService;
        }

        private void Awake()
        {
            if (_windowService is WindowService concreteService)
            {
                concreteService.RegisterLoadingScreen(this);
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}