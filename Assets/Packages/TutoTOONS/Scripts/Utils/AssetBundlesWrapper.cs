using UnityEngine;
using System.Threading.Tasks;
using System.IO;

#if GEN_ASSET_BUNDLES
using Google.Play.AssetDelivery;
#endif
namespace TutoTOONS
{
    public class AssetBundlesWrapper
    {
        public static async Task<AssetBundle> LoadAssetBundle(string _name)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
#if GEN_ASSET_BUNDLES
                PlayAssetPackRequest _asset_pack_request = PlayAssetDelivery.RetrieveAssetPackAsync(_name);
                while (!PlayAssetDelivery.IsDownloaded(_name))
                {
                    await Task.Delay(100);
                }
                AssetBundleCreateRequest _asset_bundle_request = _asset_pack_request.LoadAssetBundleAsync(_name);
                while (_asset_bundle_request.assetBundle == null)
                {
                    await Task.Delay(100);
                }
                return _asset_bundle_request.assetBundle;
#else
                return await GetAssetBundle("CustomPlatform", _name);
#endif
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return await GetAssetBundle("iOS", _name);
            }

            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
            {
                return await GetAssetBundle("Editor", _name);
            }

            Debug.Log($"Platform not supported: {Application.platform}");
            return null;
        }

        private static async Task<AssetBundle> GetAssetBundle(string _folder, string _name)
        {
            string _asset_bundle_path = Path.Combine(Application.streamingAssetsPath, "AssetBundles/" + _folder + "/");
            AssetBundleCreateRequest _asset_bundle_create_request = AssetBundle.LoadFromFileAsync(_asset_bundle_path + _name);
            while (!_asset_bundle_create_request.isDone)
            {
                await Task.Delay(100);
            }
            return _asset_bundle_create_request.assetBundle;
        }
    }
}


