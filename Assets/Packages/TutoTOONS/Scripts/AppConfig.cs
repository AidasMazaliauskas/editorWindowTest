/* Loads and parses app config. If loading from main server fails, repeats loading from backup servers.
 * If all loads fail, loading is repeated after 30 seconds timeout.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace TutoTOONS
{
    public class AppConfig
    {
        public static AppConfigInfo info;
        public static List<AppConfigLocation> ads;
        public static AppConfigSettings settings;
        public static List<AppConfigAdNetwork> ad_networks;
        public static string platform_name;
        public static UDPData udp_store_data;
        public const int version = 1;
        public const string PACKAGE_VERSION = "2021-02-25";
        public static string production_app_id = "0";
        public const string BUNDLE_ID_POSTFIX_FREE = ".free";
        public const string BUNDLE_ID_POSTFIX_FULL = ".full";
        public static string bundle_id_postfix = BUNDLE_ID_POSTFIX_FREE;

        public const string PLATFORM_NAME_ANDROID = "android";
        public const string PLATFORM_NAME_IOS = "ios";
        public const string PLATFORM_NAME_AMAZON = "amazon";
        public const string PLATFORM_NAME_UDP = "udp";
        public const string PLATFORM_NAME_HUAWEI = "huawei";
        public const string PLATFORM_NAME_UDP_SANDBOX = "UdpSandbox";
        public const string PLATFORM_NAME_UDP_ONE_STORE = "OneStore";
        public const string PLATFORM_NAME_UDP_APPTUTTI = "Apptutti";
        public const string PLATFORM_NAME_UDP_VIVEPORT = "Htc";
        public const string PLATFORM_NAME_UDP_MI_GETAPPS = "XiaomiStore";
        public const string PLATFORM_NAME_UDP_HUAWEI_APP_GALLERY = "Huawei";
        public const string PLATFORM_NAME_UDP_SAMSUNG_GALAXY_STORE = "SamsungGalaxyStore";
        public const string PLATFORM_NAME_UDP_QOOAPP_GAME_STORE = "QooApp";
        public const string PLATFORM_NAME_UDP_TPAY_MOBILE_STORE = "Tpay";
        public const string PLATFORM_NAME_UDP_UPTODOWN = "Uptodown";
        public const string PLATFORM_NAME_UDP_JIOGAMES = "JioGamesStore";

        public const string CONFIG_URL_1 = "https://apps.tutotoons.com/";
        public const string CONFIG_URL_2 = "https://apps2.tutotoons.com/";
        //public const string CONFIG_URL_3 = "https://apps3.tutotoons.com/";    //Should be removed in the future when embedded configs feature is tested and validated.

        public const string BUNDLE_ID_TUTOTOONS = "com.tutotoons.app";
        public const string BUNDLE_ID_CUTE_AND_TINY = "com.cuteandtinybabygames";
        public const string BUNDLE_ID_SPINMASTER = "com.spinmaster";
        public const string BUNDLE_ID_SUGARFREE = "games.sugarfree";
        public const int ACCOUNT_TUTOTOONS = 0;
        public const int ACCOUNT_CUTE_AND_TINY = 1;
        public const int ACCOUNT_SPINMASTER = 2;
        public const int ACCOUNT_SUGARFREE = 3;
        private const string CONFIG_FILE_NAME = "app_config";
        private const string CONFIG_FILE_NAME_ANDROID = CONFIG_FILE_NAME + ".android";
        private const string CONFIG_FILE_NAME_AMAZON = CONFIG_FILE_NAME + ".amazon";
        private const string CONFIG_FILE_NAME_IOS = CONFIG_FILE_NAME + ".ios";
        private const string CONFIG_FILE_NAME_HUAWEI = CONFIG_FILE_NAME + ".huawei";
        private const string CONFIG_FILE_NAME_JSON = CONFIG_FILE_NAME + ".json";
        private const string CONFIG_FILE_NAME_XML = CONFIG_FILE_NAME + ".xml";
        private const double CONFIG_LOAD_TIMEOUT = 30.0;
        private static double load_time;
        public static int account;
        public static bool using_embeded_config;
        public static bool initializing_app_config_data;

        public static int state;
        public const int STATE_LOADING = 0;
        public const int STATE_LOADED = 1;
        public const int STATE_FAILED = 2;
        public const int STATE_DISABLED = 3;

        public const int MAX_APP_CONFIG_LOAD_FAILS = 10;

        private static UnityWebRequest config_loader;
        private static List<string> config_urls;
        private static int url_index;
        private static double retry_timeout;
        private static int app_config_fail_counter;

        public static void Init()
        {
            platform_name = "android";
            if (Application.platform == RuntimePlatform.Android)
            {
                platform_name = PLATFORM_NAME_ANDROID;
#if GEN_PLATFORM_UDP
                //UpdatePlafromNameFromUDP();
                platform_name = UnityEngine.UDP.StoreService.StoreName.ToLower();
                if (platform_name == PLATFORM_NAME_UDP_SANDBOX.ToLower())
                {
                    platform_name = PLATFORM_NAME_UDP;
                }
#endif

#if GEN_PLATFORM_HUAWEI
                platform_name = PLATFORM_NAME_HUAWEI;
#endif
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                platform_name = PLATFORM_NAME_IOS;
            }
#if GEN_PLATFORM_AMAZON
                platform_name = PLATFORM_NAME_AMAZON;
#endif

            string _bundle_id = SystemUtils.GetBundleID();

            if (_bundle_id.IndexOf(BUNDLE_ID_TUTOTOONS) == 0)
            {
                account = ACCOUNT_TUTOTOONS;
            }
            else if (_bundle_id.IndexOf(BUNDLE_ID_CUTE_AND_TINY) == 0)
            {
                account = ACCOUNT_CUTE_AND_TINY;
            }
            else if (_bundle_id.IndexOf(BUNDLE_ID_SPINMASTER) == 0)
            {
                account = ACCOUNT_SPINMASTER;
            }
            else if (_bundle_id.IndexOf(BUNDLE_ID_SUGARFREE) == 0)
            {
                account = ACCOUNT_SUGARFREE;
            }
            else
            {
                account = ACCOUNT_TUTOTOONS;
            }

            if (_bundle_id.Contains("."))
            {
                bundle_id_postfix = _bundle_id.Substring(_bundle_id.LastIndexOf('.'));
                Debug.Log("Bundle ID postfix: " + bundle_id_postfix);
            }

            config_urls = new List<string>();
            //config_urls.Add("https://tutotoons.com/files/v3/config/test.json");    //For testing
            string url1 = CONFIG_URL_1 + _bundle_id + "." + platform_name + ".json";
            string url2 = CONFIG_URL_2 + _bundle_id + "." + platform_name + ".json";
            url1 += "?lang=" + SystemUtils.GetSystemLanguageISOCode();
            url2 += "?lang=" + SystemUtils.GetSystemLanguageISOCode();
            if (production_app_id != "0")
            {
                // Add production app ID only if it is defined
                url1 += "&production_app_id=" + production_app_id;
                url2 += "&production_app_id=" + production_app_id;                
            }
            config_urls.Add(url1);
            config_urls.Add(url2);
            //config_urls.Add(CONFIG_URL_3 + _bundle_id + "." + platform_name + ".json");
            url_index = 0;
            LoadConfig();
        }

        private static void LoadConfig()
        {
#if GEN_FREETIME
            state = STATE_DISABLED;
            AdServices.state = AdServices.STATE_DISABLED;
            Debug.Log("AppConfig.LoadConfig");
            return;
#endif

            state = STATE_LOADING;
            load_time = 0.0;
            app_config_fail_counter = 0;
            string _config_url = config_urls[url_index];
            Debug.Log("Loading config: " + _config_url);
            config_loader = UnityWebRequest.Get(_config_url);
            config_loader.SendWebRequest();
        }

        private static void UpdatePlafromNameFromUDP()
        {
#if UNITY_ANDROID
            string _path_udp_json = Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/udp/udp.json";

            if (File.Exists(_path_udp_json))
            {
                StreamReader _stream_reader = new StreamReader(_path_udp_json);
                string _data = _stream_reader.ReadToEnd();
                _stream_reader.Close();
                udp_store_data = JsonUtility.FromJson<UDPData>(_data);

                switch (udp_store_data.udpStore)
                {
                    case PLATFORM_NAME_UDP_SANDBOX:
                        platform_name = "udp";
                        break;
                    case PLATFORM_NAME_UDP_ONE_STORE:
                        platform_name = PLATFORM_NAME_UDP_ONE_STORE.ToLower();
                        break;
                    case PLATFORM_NAME_UDP_APPTUTTI:
                        platform_name = PLATFORM_NAME_UDP_APPTUTTI.ToLower();
                        break;
                    case PLATFORM_NAME_UDP_VIVEPORT:
                        platform_name = PLATFORM_NAME_UDP_VIVEPORT.ToLower();
                        break;
                    case PLATFORM_NAME_UDP_MI_GETAPPS:
                        platform_name = PLATFORM_NAME_UDP_MI_GETAPPS.ToLower();
                        break;
                    case PLATFORM_NAME_UDP_HUAWEI_APP_GALLERY:
                        platform_name = "huawei";
                        break;
                    case PLATFORM_NAME_UDP_SAMSUNG_GALAXY_STORE:
                        platform_name = PLATFORM_NAME_UDP_SAMSUNG_GALAXY_STORE.ToLower();
                        break;
                    case PLATFORM_NAME_UDP_QOOAPP_GAME_STORE:
                        platform_name = PLATFORM_NAME_UDP_QOOAPP_GAME_STORE.ToLower();
                        break;
                    case PLATFORM_NAME_UDP_TPAY_MOBILE_STORE:
                        platform_name = "tpay";
                        break;
                    case PLATFORM_NAME_UDP_UPTODOWN:
                        platform_name = PLATFORM_NAME_UDP_UPTODOWN.ToLower();
                        break;
                    case PLATFORM_NAME_UDP_JIOGAMES:
                        platform_name = PLATFORM_NAME_UDP_JIOGAMES.ToLower();
                        break;
                    default:
                        //
                        break;
                }
            }
#endif
        }

        private static void WriteConfigToFile(string _data)
        {
            if (_data == null || _data.Length == 0 || _data.Substring(0, 1) != "{")
            {
                return;
            }

            //Write config to file
            string _path = Path.Combine(Application.persistentDataPath, CONFIG_FILE_NAME_JSON);
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            File.WriteAllText(_path, _data);

            //Write config in XML format to be readable by TutoAds
            _path = Path.Combine(Application.persistentDataPath, CONFIG_FILE_NAME_XML);
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            File.WriteAllText(_path, GenerateAppConfigXML(_data));
        }

        private static bool InitData(string _data)
        {
            if (initializing_app_config_data)
            {
                return false;
            }

            initializing_app_config_data = true;


            if (_data == null || _data.Length == 0 || _data.Substring(0, 1) != "{")
            {
                initializing_app_config_data = false;
                FailedToLoad();
                return false;
            }

            AppConfigData config_data = null;

            try
            {
                config_data = JsonUtility.FromJson<AppConfigData>(_data);
            }
            catch (Exception e)
            {
                Debug.Log("AppConfig: Failed to initialize AppConfig with error: " + e.Message);
                initializing_app_config_data = false;
                FailedToLoad();
                return false;
            }

            if (config_data == null)
            {
                FailedToLoad();
                initializing_app_config_data = false;
                return false;
            }

            info = config_data.info;
            ads = config_data.ads;
            settings = config_data.settings;
            ad_networks = config_data.ad_networks;
            production_app_id = info.production_app_id;
            LogerthaWrapper.Init();

#if UNITY_IOS && !UNITY_EDITOR
            SkadNetworksController.Init();
#endif

#if GEN_SINGULAR
            SingularWrapper.Init(settings.singular_api_key, settings.singular_api_secret);
#endif

            Debug.Log("Config loaded: " + info.bundle_id);

            Debug.Log("App config rewarded_ads_enabled: " + settings.rewarded_ads_enabled);
            Debug.Log("App config interstitial_ads_enabled: " + settings.interstitial_ads_enabled);

#if GEN_SOOMLA
            SoomlaWrapper.Init();
#else
            // When Soomla is added to the project it sets AppConfig State to STATE_LOADED after 
            // it finishes initialization in SoomlaCallback class.
            state = STATE_LOADED;
            initializing_app_config_data = false;
#endif


            return true;
        }

        private static void FailedToLoad()
        {
            if (app_config_fail_counter >= MAX_APP_CONFIG_LOAD_FAILS)
            {
                state = STATE_FAILED;
                return;
            }

            app_config_fail_counter++;

            Debug.Log("Config failed to load");
            //Load next URL if available
            ++url_index;
            if (url_index < config_urls.Count)
            {
                LoadConfig();
                return;
            }

            //Failed loading config many times, disable ads to fix bugs since the internet is offline
            //AdServices.state = AdServices.STATE_DISABLED;

            //Load cached config
            string _path = Path.Combine(Application.persistentDataPath, CONFIG_FILE_NAME_JSON);
            if (File.Exists(_path))
            {
                string _data = File.ReadAllText(_path);
                if (InitData(_data))
                {
                    Debug.Log($"Config loaded from: cached");
                    LogConfigInitData("cached");
                    return;
                }
            }

            //Load embedded config
            string _config_file_name = CONFIG_FILE_NAME;
#if UNITY_ANDROID
            _config_file_name = CONFIG_FILE_NAME_ANDROID;

    #if GEN_PLATFORM_HUAWEI
            _config_file_name = CONFIG_FILE_NAME_HUAWEI;
    #endif

    #if GEN_PLATFORM_AMAZON
            _config_file_name = CONFIG_FILE_NAME_AMAZON;
    #endif
#endif

#if UNITY_IOS
            _config_file_name = CONFIG_FILE_NAME_IOS;
#endif

            TextAsset _config_text = Resources.Load<TextAsset>(_config_file_name);
            if (_config_text != null)
            {
                if (InitData(_config_text.text))
                {
                    Debug.Log($"Config loaded from: embedded");
                    info.country = "XX";
                    LogConfigInitData("embedded");
                    using_embeded_config = true;
                    return;
                }
            }

            //If no configs were loaded or they failed at parsing, repeat loading after timeout
            state = STATE_FAILED;
            url_index = 0;
            retry_timeout = 30.0;
        }

        private static string GenerateAppConfigXML(string _json_data)
        {
            try
            {
                AppConfigData _config_data = JsonUtility.FromJson<AppConfigData>(_json_data);

                string _xml_data = "";

                _xml_data += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

                _xml_data += "<data>";
                _xml_data += "<settings>";
                _xml_data += "<production_app_id>" + _config_data.info.production_app_id + "</production_app_id>";
                _xml_data += "</settings>";
                _xml_data += "</data>";

                return _xml_data;
            }
            catch (Exception e)
            {
                Debug.Log("AppConfig: Failed to generate AppConfig with error: " + e.Message);
            }

            return "";
        }

        private static string GetURLSubdomain(string _url)
        {
            Match match = Regex.Match(_url, @"(?<=\/\/)[a-z0-9]+(?=[.\/])");
            if (match != null)
            {
                if (match.Length > 0)
                {
                    return match.Value;
                }
            }
            return "undefined";
        }

        private static void LogConfigInitData(string _source)
        {
            //Debug.Log("AppConfig config source: " + _source);
            string _message = $"source {_source}";
            LogerthaWrapper.Log(LogerthaWrapper.LOG_CATEGORY_APP_CONFIG, LogerthaWrapper.LOG_OBJECT_NONE, LogerthaWrapper.LOG_TYPE_CONFIG_LOADED, _message, load_time);

            GoogleAnalytics.TrackEvent("Config Loaded", _source);
        }

        public static void Update()
        {
            switch (state)
            {
                case STATE_LOADING:
                    load_time += Time.unscaledDeltaTime;
                    if (config_loader.isDone)
                    {
                        WriteConfigToFile(config_loader.downloadHandler.text);
                        if (InitData(config_loader.downloadHandler.text))
                        {
                            Debug.Log($"Config loaded from: {config_urls[url_index]}");
                            LogConfigInitData(GetURLSubdomain(config_urls[url_index]));
                        }
                    }
#if UNITY_2020_1_OR_NEWER
                    else if (config_loader.result == UnityWebRequest.Result.ConnectionError || config_loader.result == UnityWebRequest.Result.ProtocolError || load_time >= CONFIG_LOAD_TIMEOUT)
#else
                    else if (config_loader.isNetworkError || config_loader.isHttpError || config_loader.isNetworkError || load_time >= CONFIG_LOAD_TIMEOUT)
#endif
                    {
                        FailedToLoad();
                    }
                    break;
                case STATE_LOADED:
                    break;
                case STATE_FAILED:
                    retry_timeout -= Time.unscaledDeltaTime;
                    if (retry_timeout <= 0.0)
                    {
                        LoadConfig();
                    }
                    break;
            }
        }
    }
}
