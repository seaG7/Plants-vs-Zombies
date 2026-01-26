using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.Scene
{
    public interface ISceneLoaderService
    {
        UniTask<SceneInstance> LoadScene(string sceneName,
            LoadSceneMode loadSceneMode = LoadSceneMode.Additive, bool activatedOnLoad = true);
    }
}