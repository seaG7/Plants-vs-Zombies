using System.Threading.Tasks;
using Data.Paths;
using Infrastructure.Factories.UI;
using Infrastructure.Services.Window;
using UnityEngine;

namespace Infrastructure.Services.UI
{
    public class WindowService : IWindowService
    {
        private readonly IUIFactory _uiFactory;

        public WindowService(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public bool IsWindowOpened(WindowID windowID) => _uiFactory.Exists(windowID);

        public async Task Open(WindowID windowID)
        {
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
                    canvas.sortingOrder = 999; 
                }
            }
        }

        public async Task<T> OpenAndGet<T>(WindowID windowID) where T : Component
        {
            await Open(windowID);
            return _uiFactory.GetScreenComponent<T>(windowID);
        }

        public T Get<T>(WindowID windowID) where T : Component => 
            _uiFactory.GetScreenComponent<T>(windowID);

        public void Close(WindowID windowID) => _uiFactory.DestroyScreen(windowID);

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