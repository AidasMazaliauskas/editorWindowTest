using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundlesEditor : MonoBehaviour
{
    [MenuItem("TutoTOONS/Build Asset Bundles")]
    static void BuildAssetBundles()
    {

        string _asset_bundle_path = "Assets/StreamingAssets/AssetBundles/";
        if (Directory.Exists(_asset_bundle_path))
        {
            Directory.Delete(_asset_bundle_path, true);
        }
#if UNITY_IOS
        BuildTarget _target = BuildTarget.iOS;
#else
        BuildTarget _target = BuildTarget.Android;
#endif
        string _bundle_path = _asset_bundle_path + "Editor";

        if (Directory.Exists(_bundle_path))
        {
            Directory.Delete(_bundle_path, true);
        }
        Directory.CreateDirectory(_bundle_path);
        AssetBundleManifest _out = BuildPipeline.BuildAssetBundles(_bundle_path, BuildAssetBundleOptions.ChunkBasedCompression, _target);
    }
}
