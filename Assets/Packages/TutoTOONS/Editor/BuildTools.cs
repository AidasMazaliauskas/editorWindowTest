using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using TutoTOONS;
using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System.Text;

#if GEN_PLATFORM_UDP
using UnityEngine.UDP.Editor;
using UnityEngine.UDP;
#endif

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class BuildTools
{
    public const string BUILDER_PROJECT_ID = "73379b43-bb59-48f2-bd46-d37494eda7d8";

    public const string ARG_BUNDLE_ID = "-bundle_id";
    public const string ARG_VERSION_NUMBER = "-version_number";
    public const string ARG_PRODUCT_NAME = "-product_name";
    public const string ARG_PLATFORM = "-platform";
    public const string ARG_FILENAME = "-buildFilename";
    public const string ARG_DEBUG = "debug";
    public const string ARG_CHARTBOOST = "chartboost";
    public const string ARG_SIGNING_TEAM_ID = "-signteam";
    public const string ARG_CERTIFICATE = "-certificate";
    public const string ARG_KEYSTORE = "-keystore";
    public const string ARG_TUTO_ADS = "adolf";
    public const string ARG_SUPERAWESOME = "superawesome";
    public const string ARG_KIDOZ = "kidoz";
    public const string ARG_SAFEDK = "safedk";
    public const string ARG_AD_MOB_MEDIATION = "admob";
    public const string ARG_IRONSOURCE_MEDIATION = "ironsource";
    public const string ARG_IRONSOURCE_MEDIATION_ADCOLONY = "iron_source_mediation_adcolony";
    public const string ARG_IRONSOURCE_MEDIATION_ADMOB = "iron_source_mediation_admob";
    public const string ARG_IRONSOURCE_MEDIATION_APPLOVIN = "iron_source_mediation_applovin";
    public const string ARG_IRONSOURCE_MEDIATION_UNITY_ADS = "iron_source_mediation_unity_ads";
    public const string ARG_IRONSOURCE_MEDIATION_VUNGLE = "iron_source_mediation_vungle";
    public const string ARG_IRONSOURCE_MEDIATION_INMOBI = "iron_source_mediation_inmobi";
    public const string ARG_IRONSOURCE_MEDIATION_CHARTBOOST = "iron_source_mediation_chartboost";
    public const string ARG_IRONSOURCE_MEDIATION_PANGLE = "iron_source_mediation_pangle";
    public const string ARG_IRONSOURCE_MEDIATION_FACEBOOK = "iron_source_mediation_facebook";
    public const string ARG_IRONSOURCE_MEDIATION_SUPERAWESOME = "iron_source_mediation_superaweso"; // generator limitations
    public const string ARG_GOOGLE_ANALYTICS = "google_analytics";
    public const string ARG_UNITY_ANALYTICS = "unity";
    public const string ARG_FIREBASE_ANALYTICS = "firebase_analytics";
    public const string ARG_FIREBASE_AUTH = "firebase_auth";
    public const string ARG_FIREBASE_CRASHLYTICS = "firebase_crashlytics";
    public const string ARG_FIREBASE_REALTIME_DATABASE = "firebase_realtime_database";
    public const string ARG_FIREBASE_MESSAGING = "firebase_messaging";
    public const string ARG_SINGULAR = "singular";
    public const string ARG_APP_BUNDLE = "android_app_bundle";
    public const string ARG_SOOMLA = "soomla";
    public const string ARG_FREETIME = "freetime";
    public const string ARG_SUBSCRIPTION = "subscription";
    public const string ARG_XCODE_PATH = "xcode_path";
    public const string ARG_IS_AIR = "is_air";
    public const string ARG_EXECUTION_ORDER = "-exec_order";
    public const string ARG_APPLOVIN_MAX = "applovin_max";
    public const string ARG_BUILDER = "builder";
    public const string ARG_INTRO = "intro";
    public const string ARG_ASSET_BUNDLES = "asset_bundles";
    public const string ARG_PURCHASING_PACKAGE = "in_app_purchasing";
    public const string ARG_EXPORT_SYMBOLS = "export_symbols";
    public const string ARG_HUAWEI_ADS = "huawei_ads";
    public const string ARG_COMPRESSION_METHOD = "compression_method";

    public const string COMPRESSION_METHOD_DEFAULT = "compression_type_default";
    public const string COMPRESSION_METHOD_LZ4 = "compression_type_lz4";
    public const string COMPRESSION_METHOD_LZ4HC = "compression_type_lz4hc";

    static string path_to_billing_json = "/Resources/BillingMode.json";
    static string path_to_asset_bundles = "/../AssetBundles/";
    public static string path_to_account = "../../_server/_tutoInfo.json";
    static string keystore_path = "../_generator/";

    public int callbackOrder { get { return 0; } }

    public static void ChangeAppStore(string _taget)
    {
        string _path = Application.dataPath + path_to_billing_json;
        if (!File.Exists(_path))
        {
            return;
        }
        string _store = "{\"androidStore\":\"";

        if (_taget == "android")
        {
            _store += "GooglePlay";
        }
        else if (_taget == "amazon")
        {
            _store += "AmazonAppStore";
        }
        else if (_taget == "udp")
        {
            _store += "UDP";
        }
        _store += "\"}";

        File.WriteAllText(_path, _store);
    }

    static MonoScript GetMonoScript()
    {
        string _asset_path = "Assets/Packages/TutoTOONS/TutoTOONS.prefab";
        GameObject _prefab_tuto = PrefabUtility.LoadPrefabContents(_asset_path);
        return MonoScript.FromMonoBehaviour(_prefab_tuto.GetComponent<TutoTOONS.TutoTOONS>());
    }

    public static void GetExecutionOrder()
    {
        File.WriteAllText(Application.dataPath + "/../exec_order.txt", MonoImporter.GetExecutionOrder(GetMonoScript()).ToString());
    }

    public static void Build()
    {
        BuildArguments _build_arguments = GetBuildArguments();

        if (_build_arguments.execution_order != "0")
        {
            MonoImporter.SetExecutionOrder(GetMonoScript(), int.Parse(_build_arguments.execution_order));
        }

        if (Enum.IsDefined(typeof(AndroidSdkVersions), 30))
        {
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)30;
        }

        // Get scene list
        bool _subscription_scenes_exist = false;
        int _len = EditorBuildSettings.scenes.Length;
        List<EditorBuildSettingsScene> _scenes = new List<EditorBuildSettingsScene>();
        for (int i = 0; i < _len; i++)
        {
            if (!EditorBuildSettings.scenes[i].enabled) continue;
            if (!_build_arguments.subscription)
            {
                if (EditorBuildSettings.scenes[i].path.Contains("TutoToonsSubscription")) continue;
            }
            else
            {
                if (EditorBuildSettings.scenes[i].path.Contains("TutoToonsSubscription"))
                {
                    _subscription_scenes_exist = true;
                }
            }
            _scenes.Add(EditorBuildSettings.scenes[i]);
        }
        if (_build_arguments.subscription && !_subscription_scenes_exist)
        {
            _scenes.Add(new EditorBuildSettingsScene("Assets/Packages/TutoToonsSubscription/Subscription Manager/Scenes/Device_Limit.unity", true));
            _scenes.Add(new EditorBuildSettingsScene("Assets/Packages/TutoToonsSubscription/Subscription Manager/Scenes/Parental_Gate_Screen.unity", true));
            _scenes.Add(new EditorBuildSettingsScene("Assets/Packages/TutoToonsSubscription/Subscription Manager/Scenes/Signin_Screen.unity", true));
            _scenes.Add(new EditorBuildSettingsScene("Assets/Packages/TutoToonsSubscription/Subscription Manager/Scenes/Subscription_Screen.unity", true));
        }
        EditorBuildSettingsScene[] _new_scenes = _scenes.ToArray();
        EditorBuildSettings.scenes = _new_scenes;

        BuildTarget _build_target = BuildTarget.Android;

        if (_build_arguments.platform == "android" || _build_arguments.platform == "amazon" || _build_arguments.platform == "udp" || _build_arguments.platform == "huawei")
        {
            _build_target = BuildTarget.Android;

            PlayerSettings.productName = _build_arguments.product_name;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, _build_arguments.bundle_id);
            PlayerSettings.bundleVersion = _build_arguments.version_number;
            PlayerSettings.Android.bundleVersionCode = GetVersionCode(PlayerSettings.bundleVersion);

            KeystoreData _keystore = new KeystoreData(_build_arguments.keystore);
            
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = System.IO.Path.GetFullPath(keystore_path + _keystore.keystoreName).Replace("\\", "/");
            PlayerSettings.Android.keystorePass = _keystore.keystorePassword;
            PlayerSettings.Android.keyaliasName = _keystore.keyAliasName;
            PlayerSettings.Android.keyaliasPass = _keystore.keyAliasPassword;
            SetAndroidAppBundle(_build_arguments.android_app_bundle);
            SetAndroidMinSDKVersion();
            ChangeAppStore(_build_arguments.platform);
            EditorUserBuildSettings.androidCreateSymbolsZip = _build_arguments.export_symbols;
        }
        else if (_build_arguments.platform == "ios")
        {
            _build_target = BuildTarget.iOS;

            PlayerSettings.productName = _build_arguments.product_name;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, _build_arguments.bundle_id);
            PlayerSettings.bundleVersion = _build_arguments.version_number;
            PlayerSettings.iOS.buildNumber = GetVersionCode(PlayerSettings.bundleVersion).ToString();

            PlayerSettings.iOS.appleDeveloperTeamID = _build_arguments.sign_team;
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.iOS.targetOSVersionString = "11.2";
        }

        if (_build_arguments.is_air)
        {
            if (PlayerSettings.applicationIdentifier.Substring(0, 4) != "air.")
            {
                PlayerSettings.applicationIdentifier = "air." + PlayerSettings.applicationIdentifier;
            }
        }
        else
        {
            if (PlayerSettings.applicationIdentifier.Substring(0, 4) == "air.")
            {
                PlayerSettings.applicationIdentifier = PlayerSettings.applicationIdentifier.Substring(4);
            }
        }

        if (_build_arguments.platform == "amazon")
        {
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;
        }
        else
        {
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        }

        BuildOptions _options = BuildOptions.None;

        if (_build_arguments.compression_method == COMPRESSION_METHOD_LZ4)
        {
            _options = BuildOptions.CompressWithLz4;
        }
        else if (_build_arguments.compression_method == COMPRESSION_METHOD_LZ4HC)
        {
            _options = BuildOptions.CompressWithLz4HC;
        }

        if (_build_arguments.debug)
        {
            // Unity Develment Builds shouldn't be used.
            //_options = BuildOptions.Development;
        }
        if (_build_arguments.asset_bundles)
        {
            BuildAssetBundles();
        }

#if GEN_ASSET_BUNDLES
        Google.Android.AppBundle.Editor.AssetPackConfig _asset_pack_config = new Google.Android.AppBundle.Editor.AssetPackConfig();

        foreach (string item in UnityEditor.AssetDatabase.GetAllAssetBundleNames())
        {
            _asset_pack_config.AddAssetBundle(Application.dataPath + path_to_asset_bundles + item, Google.Android.AppBundle.Editor.AssetPackDeliveryMode.InstallTime);
        }

        Google.Android.AppBundle.Editor.AssetPacks.AssetPackConfigSerializer.SaveConfig(_asset_pack_config);

        string[] _scene_names = _new_scenes.Select(o => o.path).ToArray();
        BuildPlayerOptions _build_player_options = new BuildPlayerOptions
        {
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            locationPathName =  Application.dataPath + "/../" + _build_arguments.filename,
            scenes = _scene_names,
            options = _options
        };

        Google.Android.AppBundle.Editor.Bundletool.BuildBundle(_build_player_options, _asset_pack_config);
#else
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(_new_scenes, _build_arguments.filename, _build_target, _options);
#if UNITY_ANDROID
        if (_build_arguments.export_symbols)
        {

            string _symbols_file = string.Format("{0}-{1}-v{2}.symbols.zip",
                Path.GetFileNameWithoutExtension(_build_arguments.filename),
                _build_arguments.version_number,
                GetVersionCode(PlayerSettings.bundleVersion).ToString());

            if (File.Exists(_symbols_file))
            {
                if (File.Exists("symbols.zip"))
                {
                    File.Delete("symbols.zip");
                }
                File.Move(_symbols_file, "symbols.zip");
            }
        }
#endif
#endif
    }

    private static void BuildAssetBundles()
    {
        string _asset_bundle_folder_path = "Assets/StreamingAssets/AssetBundles/";
        BuildTarget _target = BuildTarget.Android;
        string _path = "AssetBundles";

        if (Directory.Exists(_asset_bundle_folder_path))
        {
            Directory.Delete(_asset_bundle_folder_path, true);
        }
        if (Directory.Exists("AssetBundles"))
        {
            Directory.Delete("AssetBundles", true);
        }

#if UNITY_IOS
        _target = BuildTarget.iOS;
        _path = _asset_bundle_folder_path + "iOS";
#elif GEN_ASSET_BUNDLES
        _target = BuildTarget.Android;
        _path = "AssetBundles";
#else
        _target = BuildTarget.Android;
        _path = _asset_bundle_folder_path + "CustomPlatform";
#endif

        Directory.CreateDirectory(_path);
        AssetBundleManifest _out = BuildPipeline.BuildAssetBundles(_path, BuildAssetBundleOptions.ChunkBasedCompression, _target);
    }

    private static int GetVersionCode(string _version_number)
    {
        string _version_code = "0";
        string[] version_split = _version_number.Split('.');

        if (int.Parse(version_split[0]) > 1000000)
        {
            _version_code = version_split[0];
        }
        else
        {
            if (version_split.Length == 3)
            {
                _version_code = version_split[0] + version_split[1].PadLeft(3, '0') + version_split[2].PadLeft(3, '0');
            }
        }

        return int.Parse(_version_code);
    }

    static string GetAppGroup(string _certificate)
    {
        if (_certificate == "cert_apix")
            return "group.com.sugarfree.shared";
        if (_certificate == "cert_support")
            return "group.com.tutotoons.shared";
        if (_certificate == "cert_zaidimustudija")
            return "group.com.cutetinygames.shared";

        return "";
    }

    //PBX project needs to be updated after installing pods
    public static void UpdatePBXProject()
    {
#if UNITY_IOS
        BuildArguments _build_arguments = GetBuildArguments();

        PBXProject _project = new PBXProject();
        string _project_path = PBXProject.GetPBXProjectPath(_build_arguments.xcode_path);
        _project.ReadFromFile(_project_path);

        string _target_guid = _project.GetUnityMainTargetGuid();
        string _framework_guid = _project.GetUnityFrameworkTargetGuid();
        string _project_guid = _project.ProjectGuid();

        _project.AddBuildProperty(_target_guid, "OTHER_LDFLAGS", "$(fakeparameter)");

        _project.AddBuildProperty(_framework_guid, "OTHER_LDFLAGS", "$(inherited)");
        _project.AddBuildProperty(_framework_guid, "OTHER_LDFLAGS", "-ObjC");

        _project.SetBuildProperty(_target_guid, "ENABLE_BITCODE", "false");
        _project.SetBuildProperty(_project_guid, "ENABLE_BITCODE", "false");

        _project.AddCapability(_target_guid, PBXCapabilityType.AppGroups);
        _project.AddCapability(_target_guid, PBXCapabilityType.SignInWithApple);
        _project.AddCapability(_target_guid, PBXCapabilityType.InAppPurchase);

        File.WriteAllText(_project_path, _project.WriteToString());

        string _app_group = GetAppGroup(_build_arguments.certificate);

        ProjectCapabilityManager _project_capabilities = new ProjectCapabilityManager(_project_path, "Unity-iPhone/mmk.entitlements", "Unity-iPhone", _target_guid);
        string[] _app_groups = { _app_group };
        _project_capabilities.AddAppGroups(_app_groups);
        if (_build_arguments.subscription || _build_arguments.firebase_auth)
        {
            _project_capabilities.AddSignInWithApple();
            _project_capabilities.AddInAppPurchase();
        }
        if (_build_arguments.firebase_messaging)
        {
            _project_capabilities.AddPushNotifications(false);
            _project_capabilities.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        }
        _project_capabilities.WriteToFile();

        if (!_build_arguments.firebase_messaging)
        {
            string _info_plist_path = Path.Combine(_build_arguments.xcode_path, "Info.plist");
            PlistDocument _info_plist = new PlistDocument();
            _info_plist.ReadFromFile(_info_plist_path);
            PlistElementDict _root_dict = _info_plist.root;

            if (!_root_dict.values.ContainsKey("FirebaseAppDelegateProxyEnabled"))
            {
                _root_dict.SetBoolean("FirebaseAppDelegateProxyEnabled", false);
            }
            File.WriteAllText(_info_plist_path, _info_plist.WriteToString());
        }
#endif
    }

    public static void AddComponents()
    {
        UpdateUDPSettings();
        string _asset_path = "Assets/Packages/TutoTOONS/TutoTOONS.prefab";

        GameObject _prefab_tuto = PrefabUtility.LoadPrefabContents(_asset_path);

        BuildArguments _build_arguments = GetBuildArguments();

        IntegrationTestEditor.GenerateTestData(_build_arguments);

        if (_build_arguments.kidoz)
        {
            System.Type _class_type = System.Type.GetType("KidozSDK.Kidoz,Assembly-CSharp");
            _prefab_tuto.AddComponent(_class_type);
        }
        if (_build_arguments.ironsource_mediation)
        {
            System.Type _class_type = System.Type.GetType("IronSourceEvents,Assembly-CSharp");
            GameObject _go = new GameObject("IronSourceEvents");
            _go.transform.SetParent(_prefab_tuto.transform);
            _go.AddComponent(_class_type);
        }
        if (_build_arguments.singular)
        {
            System.Type _class_type = System.Type.GetType("TutoTOONS.SingularWrapper,Assembly-CSharp");
            _prefab_tuto.AddComponent(_class_type);

            System.Type _class_type2 = System.Type.GetType("SingularSDK,Assembly-CSharp");
            _prefab_tuto.AddComponent(_class_type2);
        }
        if (_build_arguments.adolf)
        {
            System.Type _class_type = System.Type.GetType("TutoAds,Assembly-CSharp");
            _prefab_tuto.AddComponent(_class_type);
        }

        if (_build_arguments.ironsource_mediation_facebook)
        {

#if GEN_IRONSOURCE_MEDIATION_FACEBOOK
            FacebookWrapper _wrapper = _prefab_tuto.AddComponent<FacebookWrapper>();
            _wrapper.app_id = TutoToonsEditor.GetFacebookAppId();
#endif
        }

        PrefabUtility.SaveAsPrefabAsset(_prefab_tuto, _asset_path);
        PrefabUtility.UnloadPrefabContents(_prefab_tuto);
    }

    public static void AddPackageManagerPackages()
    {
        BuildArguments _build_arguments = GetBuildArguments();

        if (_build_arguments.platform == "udp")
        {
            AddPackageManagerPackage("com.unity.purchasing.udp@2.0.0", "Plugins/UDP");
        }

        if (_build_arguments.purchasing_package)
        {
            AddPackageManagerPackage("com.unity.purchasing@3.2.3", "Plugins/Purchasing");
            try
            {
                Type purchasing_editor = Type.GetType("PurchasingEditor");
                MethodInfo add_tangles_method = purchasing_editor.GetMethod("AddTangleClasses");
                add_tangles_method.Invoke(null, null);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        if (_build_arguments.singular)
        {
            AddPackageManagerPackage("com.unity.nuget.newtonsoft-json@2.0.2");
        }
    }

    private static void AddPackageManagerPackage(string _identifier, string _folder_to_remove = "")
    {
        if (!string.IsNullOrEmpty(_folder_to_remove))
        {
            string path = System.IO.Path.Combine(Application.dataPath, _folder_to_remove);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            path += ".meta";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            AssetDatabase.Refresh();
        }

        AddRequest _add_package_request = Client.Add(_identifier);
        while (!_add_package_request.IsCompleted)
        {

        }
        Debug.Log($"Package '{_identifier}' request status: {_add_package_request.Status}");
        if (_add_package_request.Status >= StatusCode.Failure)
        {
            Debug.Log(_add_package_request.Error.message);
        }
    }

    public static BuildArguments GetBuildArguments()
    {
        BuildArguments _build_arguments = new BuildArguments();
        string[] _args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < _args.Length; i++)
        {
            if (_args[i] == ARG_BUNDLE_ID)
            {
                _build_arguments.bundle_id = _args[i + 1];
            }
            if (_args[i] == ARG_VERSION_NUMBER)
            {
                _build_arguments.version_number = _args[i + 1];
            }
            if (_args[i] == ARG_PRODUCT_NAME)
            {
                _build_arguments.product_name = _args[i + 1];
            }
            if (_args[i] == ARG_SIGNING_TEAM_ID)
            {
                _build_arguments.sign_team = _args[i + 1];
            }
            if (_args[i] == ARG_CERTIFICATE)
            {
                _build_arguments.certificate = _args[i + 1];
            }
            if (_args[i] == ARG_KEYSTORE)
            {
                _build_arguments.keystore = _args[i + 1];
            }
            if (_args[i] == ARG_PLATFORM)
            {
                _build_arguments.platform = _args[i + 1];
            }
            if (_args[i] == ARG_FILENAME)
            {
                _build_arguments.filename = _args[i + 1];
            }
            if (_args[i] == ARG_DEBUG)
            {
                _build_arguments.debug = true;
            }
            if (_args[i] == ARG_CHARTBOOST)
            {
                _build_arguments.chartboost = true;
            }
            if (_args[i] == ARG_TUTO_ADS)
            {
                _build_arguments.adolf = true;
            }
            if (_args[i] == ARG_SUPERAWESOME)
            {
                _build_arguments.super_awesome = true;
            }
            if (_args[i] == ARG_KIDOZ)
            {
                _build_arguments.kidoz = true;
            }
            if (_args[i] == ARG_AD_MOB_MEDIATION)
            {
                _build_arguments.admob_mediation = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION)
            {
                _build_arguments.ironsource_mediation = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_ADCOLONY)
            {
                _build_arguments.ironsource_mediation_adcolony = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_ADMOB)
            {
                _build_arguments.ironsource_mediation_admob = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_APPLOVIN)
            {
                _build_arguments.ironsource_mediation_applovin = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_UNITY_ADS)
            {
                _build_arguments.ironsource_mediation_unity_ads = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_VUNGLE)
            {
                _build_arguments.ironsource_mediation_vungle = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_INMOBI)
            {
                _build_arguments.ironsource_mediation_inmobi = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_CHARTBOOST)
            {
                _build_arguments.ironsource_mediation_chartboost = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_PANGLE)
            {
                _build_arguments.ironsource_mediation_pangle = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_FACEBOOK)
            {
                _build_arguments.ironsource_mediation_facebook = true;
            }
            if (_args[i] == ARG_IRONSOURCE_MEDIATION_SUPERAWESOME)
            {
                _build_arguments.ironsource_mediation_superawesome = true;
            }
            if (_args[i] == ARG_GOOGLE_ANALYTICS)
            {
                _build_arguments.google_analytics = true;
            }
            if (_args[i] == ARG_UNITY_ANALYTICS)
            {
                _build_arguments.unity_analytics = true;
            }
            if (_args[i] == ARG_FIREBASE_ANALYTICS)
            {
                _build_arguments.firebase_analytics = true;
            }
            if (_args[i] == ARG_SINGULAR)
            {
                _build_arguments.singular = true;
            }
            if (_args[i] == ARG_APP_BUNDLE)
            {
                _build_arguments.android_app_bundle = true;
            }
            if (_args[i] == ARG_ASSET_BUNDLES)
            {
                _build_arguments.asset_bundles = true;
            }
            if (_args[i] == ARG_XCODE_PATH)
            {
                _build_arguments.xcode_path = _args[i + 1];
            }
            if (_args[i] == ARG_IS_AIR)
            {
                _build_arguments.is_air = true;
            }
            if (_args[i] == ARG_SOOMLA)
            {
                _build_arguments.soomla = true;
            }
            if (_args[i] == ARG_SAFEDK)
            {
                _build_arguments.safedk = true;
            }
            if (_args[i] == ARG_FREETIME)
            {
                _build_arguments.freetime = true;
            }
            if (_args[i] == ARG_SUBSCRIPTION)
            {
                _build_arguments.subscription = true;
            }
            if (_args[i] == ARG_APPLOVIN_MAX)
            {
                _build_arguments.applovin_max = true;
            }
            if (_args[i] == ARG_BUILDER)
            {
                _build_arguments.builder = true;
            }
            if (_args[i] == ARG_FIREBASE_AUTH)
            {
                _build_arguments.firebase_auth = true;
            }
            if (_args[i] == ARG_FIREBASE_REALTIME_DATABASE)
            {
                _build_arguments.firebase_database = true;
            }
            if (_args[i] == ARG_FIREBASE_CRASHLYTICS)
            {
                _build_arguments.firebase_crashlytics = true;
            }
            if (_args[i] == ARG_FIREBASE_MESSAGING)
            {
                _build_arguments.firebase_messaging = true;
            }
            if (_args[i] == ARG_INTRO)
            {
                _build_arguments.intro = true;
            }
            if (_args[i] == ARG_HUAWEI_ADS)
            {
                _build_arguments.huawei_ads = true;
            }
            if (_args[i] == ARG_PURCHASING_PACKAGE)
            {
                _build_arguments.purchasing_package = true;
            }
            if (_args[i] == ARG_EXPORT_SYMBOLS)
            {
                _build_arguments.export_symbols = true;
            }
            if (_args[i] == ARG_EXECUTION_ORDER)
            {
                _build_arguments.execution_order = _args[i + 1];
            }
            if (_args[i] == ARG_COMPRESSION_METHOD)
            {
                _build_arguments.compression_method = _args[i + 1];
            }

        }
        return _build_arguments;
    }

    private static void SetAndroidAppBundle(bool _enabled)
    {
        if (_enabled)
        {
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        }
        else
        {
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        }
    }

    private static void SetAndroidMinSDKVersion()
    {
#if GEN_PLATFORM_HUAWEI
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
#else
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
#endif
    }

    public static void UpdateUDPSettings()
    {
#if GEN_PLATFORM_UDP
        // Authentication
        if (!File.Exists(path_to_account))
        {
            return;
        }
        string _tuto_info = File.ReadAllText(path_to_account);
        GeneratorTutoInfo root = JsonUtility.FromJson<GeneratorTutoInfo>(_tuto_info);
        string _json_text = "{ \"username\":\"" + root.unity_access.username + "\",\"password\":\"" + root.unity_access.password + "\",\"grant_type\":\"PASSWORD\"}";

        UnityWebRequest _access_token_request = new UnityWebRequest("https://api.unity.com/v1/core/api/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(_json_text);
        _access_token_request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        _access_token_request.downloadHandler = new DownloadHandlerBuffer();
        _access_token_request.SetRequestHeader("Content-Type", "application/json");
        _access_token_request.SendWebRequest();
        while (!_access_token_request.isDone)
        {

        }
        if (_access_token_request.result == UnityWebRequest.Result.ConnectionError || _access_token_request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error while getting Unity token: " + _access_token_request.downloadHandler.text);

            return;
        }
        AccountAccessData _account_data = JsonUtility.FromJson<AccountAccessData>(_access_token_request.downloadHandler.text);
        string _access_token = _account_data.access_token;
        string _project_id = Application.cloudProjectId;

        UnityWebRequest _www = AppStoreOnBoardApi.GetUdpToken(_access_token, _project_id);
        while (!_www.isDone)
        {

        }
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error while getting UDP token: " + _www.error);
            return;
        }
        AppStoreOnBoardApi.tokenInfo = JsonUtility.FromJson<TokenInfo>(_www.downloadHandler.text);
        UDPConfig _udp_config = TutoToonsEditor.GetUDPConfig();

        CreateAppStoreSettingsAsset(_udp_config);


        // Creating test accounts
        List<TestAccount> accounts = new List<TestAccount>();
        accounts.Add(new TestAccount() { name = "info@apix.lt", password = "apix1337" });
        accounts.Add(new TestAccount() { name = "support@tutotoons.com", password = "apix1337" });
        accounts.Add(new TestAccount() { name = "info@zaidimustudija.lt", password = "apix1337" });
        TutoToonsEditor.CreateTestAccounts(accounts, _udp_config);

        // Add items to catalog
        List<IAPConfig> _configs = TutoToonsEditor.GetIAPConfig();
        foreach (IAPConfig config in _configs)
        {
            TutoToonsEditor.AddIAPProductToCatalog(config, _udp_config);
        }
#endif
    }

    public static void CreateAppStoreSettingsAsset(UDPConfig _config)
    {
#if GEN_PLATFORM_UDP
        //File already created
        if (File.Exists(AppStoreSettings.appStoreSettingsAssetPath))
        {
            File.Delete(AppStoreSettings.appStoreSettingsAssetPath);
        }

        if (!Directory.Exists(AppStoreSettings.appStoreSettingsAssetFolder))
        {
            Directory.CreateDirectory(AppStoreSettings.appStoreSettingsAssetFolder);
        }

        var appStoreSettings = ScriptableObject.CreateInstance<AppStoreSettings>();
        appStoreSettings.AppItemId = _config.appItem.id;
        appStoreSettings.AppName = _config.appItem.name;
        appStoreSettings.AppSlug = _config.appItem.slug;
        appStoreSettings.UnityClientID = _config.appItem.clientId;
        appStoreSettings.UnityClientKey = _config.client.client_secret;
        appStoreSettings.UnityClientRSAPublicKey = _config.client.channel.publicRSAKey;
        appStoreSettings.UnityProjectID = _config.client.channel.projectGuid;
        AssetDatabase.CreateAsset(appStoreSettings, AppStoreSettings.appStoreSettingsAssetPath);

        string game_settings_prop_path = Application.dataPath + "/Plugins/Android/assets";
        string file_path = game_settings_prop_path + "/GameSettings.prop";
        string[] prop_text = {
            "*** DO NOT DELETE OR MODIFY THIS FILE !! ***",
            _config.appItem.clientId,
            "*** DO NOT DELETE OR MODIFY THIS FILE !!***" };
        if (!Directory.Exists(game_settings_prop_path))
        {
            Directory.CreateDirectory(game_settings_prop_path);
        }
        if (File.Exists(file_path))
        {
            File.Delete(file_path);
        }
        File.WriteAllLines(file_path, prop_text);
#endif
    }
}
