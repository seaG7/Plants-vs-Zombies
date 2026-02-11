// ===== Infrastructure/Services/UI/WindowService.cs =====
using System.Threading.Tasks;
using Data.Paths;
using Infrastructure.Factories.UI;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure.Services.Window
{
    /// <summary>
    /// Global window service. Allows manual registration of a preloaded loading screen.
    /// </summary>
    public class WindowService : IWindowService
    {
        private readonly IUIFactory _uiFactory;
        private LoadingWindow _preloadedLoadingScreen;

        public WindowService(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public void RegisterLoadingScreen(LoadingWindow window)
        {
            _preloadedLoadingScreen = window;
            if (_preloadedLoadingScreen != null)
            {
                Object.DontDestroyOnLoad(_preloadedLoadingScreen.gameObject);
                CheckEventSystem();
            }
        }

        public bool IsWindowOpened(WindowID windowID)
        {
            if (windowID == WindowID.Loading && _preloadedLoadingScreen != null)
                return _preloadedLoadingScreen.gameObject.activeSelf;
                
            return _uiFactory.Exists(windowID);
        }

        public async Task Open(WindowID windowID)
        {
            if (windowID == WindowID.Loading && _preloadedLoadingScreen != null)
            {
                _preloadedLoadingScreen.Show();
                return;
            }

            var path = GetWindowsPath(windowID);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"[WindowService] Path not found for ID: {windowID}");
                return;
            }
            
            var windowObj = await _uiFactory.CreateScreen(path, windowID);
            
            if (windowID == WindowID.Loading && windowObj != null)
            {
                windowObj.transform.SetParent(null);
                Object.DontDestroyOnLoad(windowObj);
                
                var canvas = windowObj.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 9999; 
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay; 
                }
                
                CheckEventSystem();
            }
        }

        public async Task<T> OpenAndGet<T>(WindowID windowID) where T : Component
        {
            await Open(windowID);
            
            if (windowID == WindowID.Loading && _preloadedLoadingScreen != null)
            {
                return _preloadedLoadingScreen.GetComponent<T>();
            }

            return _uiFactory.GetScreenComponent<T>(windowID);
        }

        public T Get<T>(WindowID windowID) where T : Component 
        {
            if (windowID == WindowID.Loading && _preloadedLoadingScreen != null)
            {
                return _preloadedLoadingScreen.GetComponent<T>();
            }
            return _uiFactory.GetScreenComponent<T>(windowID);
        }

        public void Close(WindowID windowID)
        {
            if (windowID == WindowID.Loading && _preloadedLoadingScreen != null)
            {
                _preloadedLoadingScreen.Hide();
                return;
            }
            _uiFactory.DestroyScreen(windowID);
        }

        private void CheckEventSystem()
        {
            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Object.DontDestroyOnLoad(eventSystem);
            }
        }

        private string GetWindowsPath(WindowID windowID) => windowID switch
        {
            WindowID.Loading => UIPaths.LOADING_SCREEN,
            WindowID.MainMenu => UIPaths.MAIN_MENU,
            WindowID.HUD => UIPaths.HUD,
            WindowID.Settings => UIPaths.SETTINGS,
            _ => null
        };
    }
}