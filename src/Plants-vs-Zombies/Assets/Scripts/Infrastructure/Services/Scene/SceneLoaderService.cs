using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.Scene
{
    public class SceneLoaderService : ISceneLoaderService
    {
        public async UniTask<SceneInstance> LoadScene(string sceneName, 
            LoadSceneMode loadSceneMode = LoadSceneMode.Additive, bool activatedOnLoad = true)
        {
            var loadSceneAsync = Addressables.LoadSceneAsync(sceneName, loadSceneMode, activatedOnLoad);

            await MonitorLoadProgress(loadSceneAsync);

            return loadSceneAsync.Result;
        }
        
        private static async UniTask MonitorLoadProgress(AsyncOperationHandle<SceneInstance> loadOperation)
        {
            while (!loadOperation.IsDone)
            {
                await UniTask.Yield();
            }
        }
    }
}