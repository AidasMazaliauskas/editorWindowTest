using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TutoTOONS
{
    public class LogerthaWrapper
    {
        // Logging Categories
        public const string LOG_CATEGORY_NONE = "none";
        public const string LOG_CATEGORY_APP_CONFIG = "app_config";
        public const string LOG_CATEGORY_ADS = "ads";

        //Logging objects
        public const string LOG_OBJECT_NONE = "none";
        public const string LOG_OBJECT_APP_CONFIG = "app_config";
        public const string LOG_OBJECT_ADS_CHARTBOOST = "ads_chartboost";
        public const string LOG_OBJECT_ADS_INMOBI = "ads_inmobi";
        public const string LOG_OBJECT_ADS_SUPERAWESOME = "ads_superawesome";
        public const string LOG_OBJECT_ADS_KIDOZ = "ads_kidoz";
        public const string LOG_OBJECT_ADS_ADMOB = "ads_admob";
        public const string LOG_OBJECT_ADS_APPLOVIN = "ads_applovin";
        public const string LOG_OBJECT_ADS_IRONSOURCE = "ads_ironsource";
        public const string LOG_OBJECT_ADS_FYBER = "ads_fyber";
        public const string LOG_OBJECT_ADS_AERSERV = "ads_aerserv";
        public const string LOG_OBJECT_ADS_AD_COLONY = "ads_ad_colony";
        public const string LOG_OBJECT_ADS_HEYZAP = "ads_heyzap";
        public const string LOG_OBJECT_ADS_OGURY = "ads_ogury";
        public const string LOG_OBJECT_ADS_FLYMOB = "ads_flymob";
        public const string LOG_OBJECT_ADS_REVMOB = "ads_revmob";
        public const string LOG_OBJECT_ADS_VUNGLE = "ads_vungle";
        public const string LOG_OBJECT_ADS_UNITY_ADS = "ads_unity_ads";
        public const string LOG_OBJECT_ADS_TUTOTOONS = "ads_tutotoons";
        public const string LOG_OBJECT_ADS_POPJAM = "ads_popjam";
        public const string LOG_OBJECT_ADS_PLAYABLE = "ads_playable";
        public const string LOG_OBJECT_ADS_STARTAPP = "ads_startapp";

        //Logging Types
        public const string LOG_TYPE_NONE = "none";
        public const string LOG_TYPE_ADS_FAILED_INIT = "ads_failed_init";
        public const string LOG_TYPE_ADS_FAILED_LOAD = "ads_failed_load";
        public const string LOG_TYPE_ADS_FAILED_SHOW = "ads_failed_show";
        public const string LOG_TYPE_ADS_REMOVED_SHOW_LOCK = "ads_removed_show_lock";
        public const string LOG_TYPE_CONFIG_LOADED = "config_loaded";
        public const string LOG_TYPE_AD_SHOWN = "ads_shown";
        public const string LOG_TYPE_AD_QUIT = "ads_exit_while_playing";
        public const string LOG_TYPE_AD_LOADED = "ads_loaded";

        // Global Logertha Settings
        const string LOGERTHA_TRACK_AMOUNT = "logertha_track_amount";
        private static string logging_url = "";
        private static bool enabled = false;
        private static int max_events_per_session = 3;

        private static int events_logged = 0;
        private static bool initialized = false;

        public static void Init()
        {
            logging_url = AppConfig.settings.logertha_url;
            max_events_per_session = AppConfig.settings.logertha_max_events;
            double _device_track_amount;
            if (SavedData.HasKey(LOGERTHA_TRACK_AMOUNT))
            {
                _device_track_amount = SavedData.GetDouble(LOGERTHA_TRACK_AMOUNT);
            }
            else
            {
                System.Random rand_gen = new System.Random();
                _device_track_amount = rand_gen.NextDouble();
                SavedData.SetDouble(LOGERTHA_TRACK_AMOUNT, _device_track_amount);
                Debug.Log("Initializing logertha with track amount " + _device_track_amount);
            }

            if (_device_track_amount <= AppConfig.settings.logertha_init_amount && AppConfig.settings.logertha_enabled)
            {
                enabled = true;
            }
            else
            {
                enabled = false;
            }

            initialized = true;
        }

        public static void Log(string _category, string _object, string _type, string _message, double _value = 0)
        {
            if (_type == LOG_TYPE_AD_LOADED)   // not sending ad loaded logs
            {
                return;
            }
            LogEvent(_category, _object, _type, _message, _value);
        }

        public static void LogError(string _category, string _object, string _type, string _message)
        {
            // * Category(app config, ads ir t.t.)
            // * Object(TutoAds, Kidoz, app config)
            // * Error type(failed to init, failed to load ir t.t.)
            // * Message (Max 100 char, bet reiktu labiau pasigilinti koks istikro limitas turetu buti)

            LogEvent(_category, _object, _type, _message);
        }

        private static void LogEvent(string _category, string _object, string _type, string _message, double _value = 0)
        {
#if GEN_FREETIME
            return;
#endif

            if (!enabled || events_logged >= max_events_per_session || string.IsNullOrEmpty(logging_url) || !initialized)
            {
                return;
            }

            events_logged++;

            string _url = logging_url + "?";
            _url += "category=" + _category;
            _url += "&object=" + _object;
            _url += "&type=" + _type;
            if (_value != 0)
            {
                _url += "&value=" + System.Uri.EscapeUriString(_value.ToString("0.00"));
            }
            _url += "&msg=" + System.Uri.EscapeUriString(_message);
            _url += "&plaform=" + System.Uri.EscapeUriString(AppConfig.platform_name);
            _url += "&bundle_id=" + System.Uri.EscapeUriString(SystemUtils.GetBundleID());
            _url += "&app_version=" + System.Uri.EscapeUriString(Application.version);
            _url += "&production_app_id=" + System.Uri.EscapeUriString(AppConfig.production_app_id);

            UnityWebRequest _url_request = UnityWebRequest.Get(_url);
            _url_request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (_url_request.result == UnityWebRequest.Result.ConnectionError || _url_request.result == UnityWebRequest.Result.ProtocolError)
#else
            if (_url_request.isNetworkError || _url_request.isHttpError)
#endif
            {
                // TODO: Make network error handling
                Debug.Log("Error while requesting url: " + _url + " with error: " + _url_request.error);
            }
        }

        public static string GetAdNetworkObjectByServiceID(int _ad_network_id)
        {
            switch (_ad_network_id)
            {
                case AdService.SERVICE_CHARTBOOST:
                    return LOG_OBJECT_ADS_CHARTBOOST;
                case AdService.SERVICE_INMOBI:
                    return LOG_OBJECT_ADS_INMOBI;
                case AdService.SERVICE_SUPER_AWESOME:
                    return LOG_OBJECT_ADS_SUPERAWESOME;
                case AdService.SERVICE_KIDOZ:
                    return LOG_OBJECT_ADS_KIDOZ;
                case AdService.SERVICE_AD_MOB:
                    return LOG_OBJECT_ADS_ADMOB;
                case AdService.SERVICE_APP_LOVIN:
                    return LOG_OBJECT_ADS_APPLOVIN;
                case AdService.SERVICE_IRONSOURCE:
                    return LOG_OBJECT_ADS_IRONSOURCE;
                case AdService.SERVICE_FYBER:
                    return LOG_OBJECT_ADS_FYBER;
                case AdService.SERVICE_AERSERV:
                    return LOG_OBJECT_ADS_AERSERV;
                case AdService.SERVICE_AD_COLONY:
                    return LOG_OBJECT_ADS_AD_COLONY;
                case AdService.SERVICE_HEYZAP:
                    return LOG_OBJECT_ADS_HEYZAP;
                case AdService.SERVICE_OGURY:
                    return LOG_OBJECT_ADS_OGURY;
                case AdService.SERVICE_FLYMOB:
                    return LOG_OBJECT_ADS_FLYMOB;
                case AdService.SERVICE_REVMOB:
                    return LOG_OBJECT_ADS_REVMOB;
                case AdService.SERVICE_VUNGLE:
                    return LOG_OBJECT_ADS_VUNGLE;
                case AdService.SERVICE_UNITY:
                    return LOG_OBJECT_ADS_UNITY_ADS;
                case AdService.SERVICE_TUTOTOONS:
                    return LOG_OBJECT_ADS_TUTOTOONS;
                case AdService.SERVICE_POPJAM:
                    return LOG_OBJECT_ADS_POPJAM;
                case AdService.SERVICE_PLAYABLE:
                    return LOG_OBJECT_ADS_PLAYABLE;
                case AdService.SERVICE_STARTAPP:
                    return LOG_OBJECT_ADS_STARTAPP;
            }
            return LOG_OBJECT_NONE;
        }

    }
}
