using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Infrastructure.Providers.AssetsAddressables
{
    public interface IAssetsAddressablesProvider
    {
        UniTask<T> GetAsset<T>(string address) where T : Object;
        UniTask<T> GetAsset<T>(AssetReference assetReference) where T : Object;
        UniTask<List<T>> GetAssets<T>(IEnumerable<string> addresses) where T : Object;
        void CleanUp();
    }
}