using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GEN_IRONSOURCE_MEDIATION_FACEBOOK
using Facebook.Unity;
#endif

namespace TutoTOONS
{
    public class AdServices
    {
        public delegate void OnAdFinished(bool _success);

        public const int STATE_DISABLED = 0;
        public const int STATE_LOADING_CONFIG = 1;
        public const int STATE_WAITING_FOR_PRIVACY_SETTINGS = 2;
        public const int STATE_ENABLED = 3;
        public const int STATE_SHOWING_AD = 4;
        public static int state;

        public static List<AdData> unique_ads = new List<AdData>();
        private static List<AdService> ad_services = new List<AdService>();
        private static List<AdLocation> ad_locations = new List<AdLocation>();
        private static IDictionary<string, List<int>> ad_priorities = new Dictionary<string, List<int>>();
        private static AdPreferences ad_load_preferences;

        public static AdService active_service;
        public static double forced_ads_timeout;
        public static double forced_location_branch_end_timeout;
        public static int next_ad_delay;
        public static int forced_location_delay;
        public static bool ad_completed;
        public static bool muted;

        public static int interstitials_shown;
        public static int interstitial_videos_shown;
        public static int rewarded_videos_shown;
        public static int total_shown;

        public static bool age_over_kids_protection_limit { get; private set; } = false;
        public static bool consent_to_collect_data { get; private set; } = false;
        public static int user_age { get; private set; } = 1;
        public static bool ad_services_initialized = false;
        private static bool allow_to_change_privacy_settings = false;
        private static bool is_privacy_settings_collected = false;
        private static bool is_privacy_init_locked = false;
        private static float adShowingStateTimer;
        private static DateTime start_show_ad;

        private static OnAdFinished m_onAdFinishedCallback;

        public static void Init()
        {
            forced_ads_timeout = 0;
            forced_location_branch_end_timeout = 0;
            next_ad_delay = 90;
            forced_location_delay = 600;
            muted = false;
            ReadPrivacySettings();

            if (state != STATE_WAITING_FOR_PRIVACY_SETTINGS)
            {
                state = STATE_LOADING_CONFIG;
            }
        }

        public static void ReadAppConfig()
        {
            #if GEN_IRONSOURCE_MEDIATION_FACEBOOK
            if(!FB.IsInitialized)
            {
                return;
            }
            #endif

            ad_services_initialized = true;

            if (AppConfig.settings.override_privacy_comp)
            {
                if (AppConfig.settings.override_age_settings)
                {
                    age_over_kids_protection_limit = false;
                    user_age = 1;
                }
                if (AppConfig.settings.override_data_settings)
                {
                    consent_to_collect_data = false;
                }
            }

            if (!age_over_kids_protection_limit && consent_to_collect_data)
            {
                consent_to_collect_data = false;
                SetDataCollectionConsent(false);
            }

            Debug.Log(String.Format("Initializing AdServices with age over {0}, age {1}, data {2}", age_over_kids_protection_limit, user_age, consent_to_collect_data));

            #if BUILDER_UNITY
            if (IAPController.ProductIsPurchased(ProductTypeIAP.NoAds))
            {
                state = STATE_DISABLED;
                return;
            }
            #endif

            if (!AppConfig.settings.ads_enabled)
            {
                state = STATE_DISABLED;
                return;
            }
            if (ad_services == null)
            {
                ad_services = new List<AdService>();
            }

            //Create ad priorities
            if (AppConfig.settings.ad_priorities != null)
            {
                for (int i = 0; i < AppConfig.settings.ad_priorities.Count; i++)
                {
                    AppConfigAdPriorities _priorities = AppConfig.settings.ad_priorities[i];
                    ad_priorities[_priorities.type] = _priorities.ad_networks;
                }
            }

            //Create ad services
            if (!Application.isEditor)
            {
                for (int i = 0; i < AppConfig.ad_networks.Count; i++)
                {
                    AppConfigAdNetwork _ad_network = AppConfig.ad_networks[i];
                    if (_ad_network.settings != null)
                    {
                        if (!_ad_network.settings.enabled)
                        {
                            continue;
                        }
                    }
                    AdService _service = null;
                    bool _already_added = false;
                    //Check maybe service is already initialized
                    for (int j = 0; j < ad_services.Count; j++)
                    {
                        if (_ad_network.ad_network_id == ad_services[j].service_id)
                        {
                            _service = ad_services[j];
                            _already_added = true;
                        }
                    }
                    //Create ad service
                    if (_service == null)
                    {
                        string _wrapper_class_name = null;
                        if (_ad_network.ad_network_id == AdService.SERVICE_CHARTBOOST)
                        {
                            _wrapper_class_name = "Chartboost";
                        }
                        else if (_ad_network.ad_network_id == AdService.SERVICE_SUPER_AWESOME)
                        {
                            _wrapper_class_name = "SuperAwesome";
                        }
                        else if (_ad_network.ad_network_id == AdService.SERVICE_KIDOZ)
                        {
                            _wrapper_class_name = "Kidoz";
                        }
                        else if (_ad_network.ad_network_id == AdService.SERVICE_TUTOTOONS)
                        {
                            _wrapper_class_name = "TutoToons";
                        }
                        else if (_ad_network.ad_network_id == AdService.SERVICE_AD_MOB)
                        {
                            _wrapper_class_name = "AdMob";
                        }
                        else if (_ad_network.ad_network_id == AdService.SERVICE_APP_LOVIN)
                        {
                            _wrapper_class_name = "ApplovinMax";
                        }
                        else if (_ad_network.ad_network_id == AdService.SERVICE_UNITY)
                        {
                            _wrapper_class_name = "UnityAds";
                        }
                        else if (_ad_network.ad_network_id == AdService.SERVICE_IRONSOURCE)
                        {
                            _wrapper_class_name = "IronSource";
                        }
                        else if (_ad_network.ad_network_id == AdService.SERVICE_STARTAPP)
                        {
                            _wrapper_class_name = "StartApp";
                        }
                        #if GEN_PLATFORM_UDP || GEN_PLATFORM_HUAWEI
                        else if (_ad_network.ad_network_id == AdService.SERVICE_HUAWEI)
                        {
                            _wrapper_class_name = "HuaweiAds";
                        }
                        #endif
                        if (_wrapper_class_name != null)
                        {
                            _wrapper_class_name = "TutoTOONS." + _wrapper_class_name + "Wrapper";
                            //Debug.Log("Class name: " + _wrapper_class_name);
                            System.Type _wrapper_class = System.Type.GetType(_wrapper_class_name);
                            //Debug.Log("Class: " + _wrapper_class);
                            if (_wrapper_class != null)
                            {
                                _service = (AdService)System.Activator.CreateInstance(_wrapper_class);
                                //Debug.Log("Instance: " + _service);
                            }
                        }
                    }
                    if (_service != null)
                    {
                        if (age_over_kids_protection_limit)
                        {
                            if (!_ad_network.settings.allow_no_comp)
                            {
                                continue;
                            }
                        }
                        _service.SetData(_ad_network);
                        if (!_already_added)
                        {
                            try
                            {
                                _service.Init();
                                ad_services.Add(_service);
                            }
                            catch (Exception e)
                            {
                                LogerthaWrapper.LogError(LogerthaWrapper.LOG_CATEGORY_ADS, LogerthaWrapper.GetAdNetworkObjectByServiceID(_ad_network.ad_network_id), LogerthaWrapper.LOG_TYPE_ADS_FAILED_INIT, e.Message);
                                Debug.Log("Failed to add ad service ID: " + _ad_network.title + " with error: " + e.Message);
                            }
                        }
                    }
                }
            }

            //Create ad locations
            for (int i = 0; i < AppConfig.ads.Count; i++)
            {
                AppConfigLocation _location_data = AppConfig.ads[i];
                for (int j = 0; j < _location_data.ad.Count; j++)
                {
                    AppConfigAd _ad_data = _location_data.ad[j];
                    if (!_ad_data.enabled)
                    {
                        continue;
                    }
                    AdLocation _ad_location = new AdLocation();
                    _ad_location.keyword = new AdLocationKeyword(_location_data.keyword);
                    _ad_location.location_id = _location_data.location_id;
                    _ad_location.campaign_id = _ad_data.campaign_id;
                    _ad_location.ad_id = _ad_data.ad_id;
                    _ad_location.fill_rate = _ad_data.fill_rate;
                    _ad_location.priority = _ad_data.priority;
                    ad_locations.Add(_ad_location);
                    AdData _ad = null;
                    for (int k = 0; k < unique_ads.Count; k++)
                    {
                        if (unique_ads[k].ad_network_id == _ad_data.ad_network_id && unique_ads[k].key == _ad_data.key && unique_ads[k].type == _ad_data.type)
                        {
                            _ad = unique_ads[k];
                            _ad.ad_location = _ad_location;
                            break;
                        }
                    }
                    if (_ad == null)
                    {
                        _ad = new AdData(_ad_data.ad_network_id, _ad_data.key, _ad_data.key_no_comp, _ad_data.type, _ad_location);
                        unique_ads.Add(_ad);
                    }
                    _ad_location.ad = _ad;
                }
            }

            //Assign ad services, remove ads which don't have ad service
            for (int i = 0; i < unique_ads.Count; i++)
            {
                for (int j = 0; j < ad_services.Count; j++)
                {
                    if (unique_ads[i].ad_network_id == ad_services[j].service_id)
                    {
                        unique_ads[i].SetAdService(ad_services[j]);
                        ad_services[j].ads.Add(unique_ads[i]);
                        break;
                    }
                }
                if (unique_ads[i].ad_service == null)
                {
                    unique_ads.RemoveAt(i);
                    --i;
                }
            }

            SetupTestAds();

            // Reading and setting ad timeouts
            forced_ads_timeout = AppConfig.settings.first_ad_delay;
            next_ad_delay = AppConfig.settings.next_ad_delay;
            forced_location_delay = AppConfig.settings.forced_video_ad_interval;
            forced_location_branch_end_timeout = AppConfig.settings.forced_video_ad_interval;

            if (ad_services.Count > 0 && unique_ads.Count > 0)
            {
                if (state != STATE_DISABLED)
                {
                    Debug.Log("AdServices.ReadAppConfig 3");
                    state = STATE_ENABLED;
                }
            }
            else
            {
                state = STATE_DISABLED;
            }
        }

        private static void SetupTestAds()
        {
            #if GEN_TEST_ADS || UNITY_EDITOR
            unique_ads.Clear();

            AdService _test_service = (AdService)System.Activator.CreateInstance(System.Type.GetType("TutoTOONS.TestAdsWrapper"));
            _test_service.title = "Test ads";
            _test_service.Init();
            ad_services.Add(_test_service);

            for (int i = 0; i < AppConfig.ads.Count; i++)
            {
                AppConfigLocation _location_data = AppConfig.ads[i];
                for (int j = 0; j < _location_data.ad.Count; j++)
                {
                    AppConfigAd _ad_data = _location_data.ad[j];
                    if (!_ad_data.enabled)
                    {
                        continue;
                    }
                    AdLocation _ad_location = new AdLocation();
                    _ad_location.keyword = new AdLocationKeyword(_location_data.keyword);
                    _ad_location.location_id = _location_data.location_id;
                    _ad_location.campaign_id = _ad_data.campaign_id;
                    _ad_location.ad_id = _ad_data.ad_id;
                    _ad_location.fill_rate = _ad_data.fill_rate;
                    _ad_location.priority = _ad_data.priority;
                    ad_locations.Add(_ad_location);
                    AdData _ad = null;
                    for (int k = 0; k < unique_ads.Count; k++)
                    {
                        if (unique_ads[k].ad_network_id == _ad_data.ad_network_id && unique_ads[k].key == _ad_data.key && unique_ads[k].type == _ad_data.type)
                        {
                            _ad = unique_ads[k];
                            _ad.ad_location = _ad_location;
                            break;
                        }
                    }
                    if (_ad == null)
                    {
                        _ad = new AdData(_ad_data.ad_network_id, _ad_data.key, _ad_data.key_no_comp, _ad_data.type, _ad_location);
                        unique_ads.Add(_ad);
                    }
                    _ad_location.ad = _ad;
                }
            }

            for (int i = 0; i < unique_ads.Count; i++)
            {
                for (int j = unique_ads.Count - 1; j > 0 && i < j; j--)
                {
                    if (unique_ads[i].type == unique_ads[j].type)
                    {
                        unique_ads.RemoveAt(j);
                    }
                }
            }

            for (int i = 0; i < unique_ads.Count; i++)
            {
                for (int j = 0; j < ad_services.Count; j++)
                {


                    unique_ads[i].SetAdService(ad_services[j]);
                    ad_services[j].ads.Add(unique_ads[i]);
                    ad_services[j].SetAdLoaded(null, unique_ads[i].type);
                }
            }
            #endif
        }

        public static bool CanShowAd(AdLocationKeyword _location)
        {
            if (state != STATE_ENABLED)
            {
                return false;
            }
            // Don't need to check for forced ads in Unity games. Games can manage this themselves.
            if (AdLocation.IsLocationForced(_location.getKeyword()))
            {
                if (!CanShowForcedAd(_location.getKeyword()))
                {
                    return false;
                }
            }
            // If app type is FULL, allow only soft locations (panel ad and movie theater)
            if (AppConfig.bundle_id_postfix == AppConfig.BUNDLE_ID_POSTFIX_FULL)
            {
                if (!AdLocation.IsLocationSoft(_location.getKeyword()))
                {
                    return false;
                }
            }

            for (int i = 0; i < ad_locations.Count; i++)
            {
                // check interstitial setting
                string _ad_type = ad_locations[i].ad.type;
                if (!AppConfig.settings.interstitial_ads_enabled && (_ad_type == AdData.TYPE_INTERSTITIAL || _ad_type == AdData.TYPE_INTERSTITIAL_VIDEO || _ad_type == AdData.TYPE_STATIC))
                {
                    continue;
                }

                // check rewarded setting
                if (!AppConfig.settings.rewarded_ads_enabled && _ad_type == AdData.TYPE_VIDEO)
                {
                    continue;
                }

                if (ad_locations[i].keyword.getKeyword() == _location.getKeyword() && ad_locations[i].ad.state == AdData.STATE_LOADED && ad_locations[i].ad.ad_service.CanShowAd(ad_locations[i].ad))
                {
                    if (AdLocation.IsLocationForced(ad_locations[i].keyword.getKeyword()))
                    {
                        if (!CanShowForcedAd(ad_locations[i].keyword.getKeyword()))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool ShowAd(AdLocationKeyword _location, OnAdFinished _onAdFinishedCallback = null)
        {
            if (state != STATE_ENABLED)
            {
                return false;
            }

            if (!CanShowAd(_location))
            {
                return false;
            }

            List<AdLocation> _locations = SelectLocations(_location.getKeyword());
            for (int i = 0; i < _locations.Count; i++)
            {
                try
                {
                    _locations[i].ad.UpdateAdLocation(_locations[i]);

                    if (_locations[i].ad.ad_service.ShowAd(_locations[i].ad))
                    {
                        start_show_ad = DateTime.Now;
                        Debug.Log("AdServices.ShowAd, service: " + _locations[i].ad.ad_service.service_id + ", no_comp: " + age_over_kids_protection_limit + ", data: " + consent_to_collect_data);

                        active_service = _locations[i].ad.ad_service;

                        if (_location.getKeyword().Equals(AdLocation.KEYWORD_BRANCH_END.getKeyword()))
                        {
                            ResetForcedLocationBranchEndTimeout();
                        }

                        m_onAdFinishedCallback = _onAdFinishedCallback;
                        state = STATE_SHOWING_AD;

                        return true;
                    }
                }
                catch (Exception e)
                {
                    LogerthaWrapper.LogError(LogerthaWrapper.LOG_CATEGORY_ADS, LogerthaWrapper.GetAdNetworkObjectByServiceID(_locations[i].ad.ad_service.service_id), LogerthaWrapper.LOG_TYPE_ADS_FAILED_SHOW, e.Message);
                    Debug.Log("Failed to show ad from ad service ID: " + _locations[i].ad.ad_service.title + " with error: " + e.Message);
                }
            }

            return false;
        }

        public static void HidePanelAd(bool show_animation)
        {
            if (ad_locations == null)
            {
                return;
            }

            for (int i = 0; i < ad_services.Count; i++)
            {
                ad_services[i].HidePanelAd(show_animation);
            }
        }

        public static void HideBannerAd()
        {
            if (ad_locations == null)
            {
                return;
            }

            for (int i = 0; i < ad_services.Count; i++)
            {
                ad_services[i].HideBannerAd();
            }
        }

        public static void SetAdLoadPreferences(AdPreferences _ad_load_preferences)
        {
            ad_load_preferences = _ad_load_preferences;
        }

        private static bool CanShowForcedAd(string _location)
        {
            if (!AdLocation.IsLocationForced(_location))
            {
                return false;
            }

            if (_location.Equals(AdLocation.KEYWORD_BRANCH_END.getKeyword()))
            {
                //Branch End ads
                if (forced_location_branch_end_timeout < 0 && forced_ads_timeout < 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //All other forced ads
                if (forced_ads_timeout < 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static void ResetForcedAdTimeout()
        {
            if (forced_ads_timeout < next_ad_delay)
            {
                forced_ads_timeout = next_ad_delay;
            }
        }

        private static void ResetForcedLocationBranchEndTimeout()
        {
            if (forced_location_branch_end_timeout < forced_location_delay)
            {
                forced_location_branch_end_timeout = forced_location_delay;
            }
        }

        private static List<AdLocation> SelectLocations(string _location_keyword)
        {
            //Get list of available ads
            List<AdLocation> _locations = new List<AdLocation>();
            for (int i = 0; i < ad_locations.Count; i++)
            {
                if (ad_locations[i].keyword.getKeyword() == _location_keyword && ad_locations[i].ad.state == AdData.STATE_LOADED && ad_locations[i].ad.ad_service.CanShowAd(ad_locations[i].ad))
                {
                    _locations.Add(ad_locations[i]);
                }
            }
            if (_locations.Count <= 1)
            {
                return _locations;
            }

            //Arrange by priority
            bool _swapped;
            do
            {
                _swapped = false;
                for (int i = 0; i < _locations.Count - 1; i++)
                {
                    if (_locations[i].priority > _locations[i + 1].priority)
                    {
                        AdLocation _tmp = _locations[i];
                        _locations[i] = _locations[i + 1];
                        _locations[i + 1] = _tmp;
                        _swapped = true;
                    }
                }
            } while (_swapped);

            //Arrange by fill rate
            double _fill_rate_sum = 0.0;
            for (int i = 0; i < _locations.Count; i++)
            {
                _fill_rate_sum += _locations[i].fill_rate;
            }
            double _current_fill_rate = 0.0;
            double _rand_fill_rate = ((double)UnityEngine.Random.Range(0, 0xFFFF)) / 0x10000 * _fill_rate_sum;
            for (int i = 0; i < _locations.Count; i++)
            {
                _current_fill_rate += _locations[i].fill_rate;
                if (_rand_fill_rate <= _current_fill_rate)
                {
                    if (i > 0)
                    {
                        AdLocation _tmp = _locations[0];
                        _locations[0] = _locations[i];
                        _locations[i] = _tmp;
                    }
                    break;
                }
            }

            //Arrange by ad network priorities
            if (ad_priorities.ContainsKey(_locations[0].ad.type))
            {
                List<int> _priorities = ad_priorities[_locations[0].ad.type];
                bool _priority_found = false;
                for (int i = 0; i < _priorities.Count && !_priority_found; i++)
                {
                    for (int j = 0; j < _locations.Count && !_priority_found; j++)
                    {
                        if (_priorities[i] == _locations[j].ad.ad_network_id)
                        {
                            _priority_found = true;
                            _priorities.RemoveAt(i);
                            if (j > 0)
                            {
                                AdLocation _tmp = _locations[0];
                                _locations[0] = _locations[j];
                                _locations[j] = _tmp;
                            }
                        }
                    }
                }
            }

            return _locations;
        }

        private static void HandleAudio()
        {
            if (active_service != null)
            {
                if (active_service.state == AdService.STATE_SHOWING_AD)
                {
                    AudioListener.volume = 0.0f;
                }
                else
                {
                    AudioListener.volume = 1.0f;
                }
            }
            else
            {
                AudioListener.volume = 1.0f;
            }
        }

        public static string GetAdsLog()
        {
            string log = GetAdsLogForLocation(AdLocation.KEYWORD_BETWEEN_SCENES.getKeyword());
            log += GetAdsLogForLocation(AdLocation.KEYWORD_MOVIE_THEATER.getKeyword());
            return log;
        }

        public static string GetAdsLogForLocation(string location)
        {
            string log = "";
            //Debug.LogWarning("GetAdsLogForLocation location " + location + " ad_locations " + ad_locations);
            if (ad_locations == null)
            {
                return log;
            }
            foreach (AdLocation ad_location in ad_locations)
            {
                //Debug.LogWarning("GetAdsLogForLocation ad_location.keyword == location " + ad_location.keyword == location);
                if (ad_location.keyword.getKeyword() == location)
                {
                    //Debug.LogWarning("GetAdsLogForLocation ad_location.ad.ad_service " + ad_location.ad.ad_service);
                    if (ad_location.ad.ad_service != null)
                    {
                        string ad_state = "<color=red>-</color>";
                        if (ad_location.ad.state == AdData.STATE_LOADING)
                        {
                            ad_state = "?";
                        }
                        else if (ad_location.ad.state == AdData.STATE_LOADED)
                        {
                            ad_state = "<color=green>+</color>";
                        }
                        log += location + ": " + ad_location.ad.ad_service.title + " " + ad_location.ad.type + " (<b>" + ad_state + "</b>)\n";
                    }
                }
            }
            return log;
        }

        public static void UnlockPrivacyInit()
        {
            is_privacy_init_locked = false;
        }

        public static void WaitForPrivacySettings(bool _lock_privacy_init = false)
        {
            if (!ad_services_initialized && state == STATE_DISABLED)
            {
                is_privacy_init_locked = _lock_privacy_init;
                allow_to_change_privacy_settings = true;
                state = STATE_WAITING_FOR_PRIVACY_SETTINGS;
                Debug.Log("AdServices.WaitForPrivacySettings: Initialized successfully!");
            }
            else
            {
                Debug.Log("AdServices.WaitForPrivacySettings: Failed to initialize.");
            }
        }

        public static void SetAgeOverKidsProtectionLimit(bool _is_age_over_kids_protection_limit)
        {
            if (!allow_to_change_privacy_settings)
            {
                return;
            }
            if (AppConfig.settings != null)
            {
                if (!(AppConfig.settings.override_privacy_comp && AppConfig.settings.override_age_settings))
                {
                    age_over_kids_protection_limit = _is_age_over_kids_protection_limit;
                }
            }
            else
            {
                age_over_kids_protection_limit = _is_age_over_kids_protection_limit;
            }
            SavePrivacyAgeSetting(_is_age_over_kids_protection_limit);
        }

        public static void SetDataCollectionConsent(bool _gave_consent_to_collect_data)
        {
            if (!allow_to_change_privacy_settings)
            {
                return;
            }
            if (AppConfig.settings != null)
            {
                if (!(AppConfig.settings.override_privacy_comp && AppConfig.settings.override_data_settings))
                {
                    consent_to_collect_data = _gave_consent_to_collect_data;
                }
            }
            else
            {
                consent_to_collect_data = _gave_consent_to_collect_data;
            }
            SaveDataCollectionConsentSetting(_gave_consent_to_collect_data);
            Utils.Debug.Console.DebugConsole.instance?.console?.content_ads?.UpdateInfo();
        }

        public static void SetUserAge(int _user_age)
        {
            if (!allow_to_change_privacy_settings)
            {
                return;
            }
            if (AppConfig.settings != null)
            {
                if (!(AppConfig.settings.override_privacy_comp && AppConfig.settings.override_age_settings))
                {
                    user_age = _user_age;
                }
            }
            else
            {
                user_age = _user_age;
            }
            SaveUserAge(_user_age);
            Utils.Debug.Console.DebugConsole.instance?.console?.content_ads?.UpdateInfo();
        }

        public static void PrivacySettingsCollected()
        {
            if (!allow_to_change_privacy_settings)
            {
                return;
            }
            is_privacy_settings_collected = true;
            SavePrivacySettingsCollected(is_privacy_settings_collected);
        }

        private static void SavePrivacyAgeSetting(bool _is_age_over_kids_protection_limit)
        {
            SavedData.HasKey("age_over_kids_protection_limit");

            if (_is_age_over_kids_protection_limit)
            {
                SavedData.SetInt("age_over_kids_protection_limit", 1);
            }
            else
            {
                SavedData.SetInt("age_over_kids_protection_limit", 0);
            }
        }

        private static void SaveDataCollectionConsentSetting(bool _gave_consent_to_collect_data)
        {
            SavedData.HasKey("gave_consent_to_collect_data");

            if (_gave_consent_to_collect_data)
            {
                SavedData.SetInt("gave_consent_to_collect_data", 1);
            }
            else
            {
                SavedData.SetInt("gave_consent_to_collect_data", 0);
            }
        }

        private static void SavePrivacySettingsCollected(bool _privacy_settings_collected)
        {
            SavedData.HasKey("privacy_settings_collected");

            if (_privacy_settings_collected)
            {
                SavedData.SetInt("privacy_settings_collected", 1);
            }
            else
            {
                SavedData.SetInt("privacy_settings_collected", 0);
            }
        }

        private static void SaveUserAge(int _user_age)
        {
            SavedData.HasKey("age_privacy");
            SavedData.SetInt("age_privacy", _user_age);
        }

        private static void ReadPrivacySettings()
        {
            if (SavedData.HasKey("age_over_kids_protection_limit"))
            {
                age_over_kids_protection_limit = SavedData.GetInt("age_over_kids_protection_limit", 0) > 0;
            }

            if (SavedData.HasKey("gave_consent_to_collect_data"))
            {
                consent_to_collect_data = SavedData.GetInt("gave_consent_to_collect_data", 0) > 0;
            }
            if (SavedData.HasKey("age_privacy"))
            {
                user_age = SavedData.GetInt("age_privacy", 1);
            }
            if (SavedData.HasKey("privacy_settings_collected"))
            {
                is_privacy_settings_collected = SavedData.GetInt("privacy_settings_collected", 0) > 0;
            }
        }

        public static void OnApllicationPause(bool _paused)
        {
            foreach (AdService _item in ad_services)
            {
                _item.OnApplicationPause(_paused);
            }
            if (_paused) 
            {
                if (active_service != null)
                {
                    if (active_service.state == AdService.STATE_SHOWING_AD)
                    {
                        LogerthaWrapper.Log(LogerthaWrapper.LOG_CATEGORY_ADS, LogerthaWrapper.GetAdNetworkObjectByServiceID(active_service.service_id), LogerthaWrapper.LOG_TYPE_AD_QUIT, "none");
                    }
                } 
            }
        }

        public static void Update()
        {
            double dt = Time.unscaledDeltaTime;

            //Timeouts
            forced_ads_timeout -= dt;
            forced_location_branch_end_timeout -= dt;

            HandleAudio();

            //Update ad services
            if (state != STATE_DISABLED && state != STATE_LOADING_CONFIG && ad_services != null)
            {
                for (int i = 0; i < ad_services.Count; i++)
                {
                    ad_services[i].Update();
                }
            }
            #if UNITY_ANDROID
            if (state == STATE_SHOWING_AD)
            {
                if (adShowingStateTimer <= 0f)
                {
                    adShowingStateTimer = 60f;
                }
                else
                {
                    adShowingStateTimer -= Time.unscaledDeltaTime;

                    if (adShowingStateTimer <= 0f)
                    {
                        state = STATE_ENABLED;
                    }
                }
            }
            else
            {
                adShowingStateTimer = 0f;
            }
            #endif
            switch (state)
            {
                case STATE_DISABLED:
                    if (m_onAdFinishedCallback != null)
                    {
                        m_onAdFinishedCallback(ad_completed);
                        m_onAdFinishedCallback = null;
                    }
                    break;
                case STATE_LOADING_CONFIG:
                    if (AppConfig.state == AppConfig.STATE_LOADED)
                    {
                        ReadAppConfig();
                    }
                    break;
                case STATE_WAITING_FOR_PRIVACY_SETTINGS:
                    if (AppConfig.state == AppConfig.STATE_LOADED && is_privacy_settings_collected && !is_privacy_init_locked)
                    {
                        ReadAppConfig();
                    }
                    break;
                case STATE_ENABLED:
                    //Preload ads
                    for (int i = 0; i < unique_ads.Count; i++)
                    {
                        unique_ads[i].preload_timeout -= dt;

                        if (unique_ads[i].preload_timeout <= 0.0)
                        {
                            if (unique_ads[i].state == AdData.STATE_LOADING)
                            {
                                unique_ads[i].SetEmpty();
                                continue;
                            }

                            if (!unique_ads[i].CanLoadAd())
                            {
                                continue;
                            }

                            try
                            {
                                unique_ads[i].ad_service.LoadAd(unique_ads[i], ad_load_preferences);
                            }
                            catch (Exception e)
                            {
                                LogerthaWrapper.LogError(LogerthaWrapper.LOG_CATEGORY_ADS, LogerthaWrapper.GetAdNetworkObjectByServiceID(unique_ads[i].ad_service.service_id), LogerthaWrapper.LOG_TYPE_ADS_FAILED_LOAD, e.Message);
                                Debug.Log("Failed to load ad from ad service ID: " + unique_ads[i].ad_service.title + " with error: " + e.Message);
                            }
                        }
                    }
                    break;
                case STATE_SHOWING_AD:
                    if (active_service.state != AdService.STATE_SHOWING_AD)
                    {
                        ad_completed = active_service.ad_completed;

                        ResetForcedAdTimeout();

                        double _ad_shown_for = DateTime.Now.Subtract(start_show_ad).TotalSeconds;
                        string _message = $"completed {active_service.ad_completed}";
                        LogerthaWrapper.Log(LogerthaWrapper.LOG_CATEGORY_ADS, LogerthaWrapper.GetAdNetworkObjectByServiceID(active_service.service_id), LogerthaWrapper.LOG_TYPE_AD_SHOWN, _message, _ad_shown_for);

                        if (m_onAdFinishedCallback != null)
                        {
                            m_onAdFinishedCallback(ad_completed);
                            m_onAdFinishedCallback = null;
                        }

                        active_service = null;
                        state = STATE_ENABLED;
                    }
                    break;
            }
        }

    }
}
