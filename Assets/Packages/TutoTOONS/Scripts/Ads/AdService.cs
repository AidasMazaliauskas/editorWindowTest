using System;
using System.Collections.Generic;
using UnityEngine;
using TutoTOONS.Utils.Debug.Console;

namespace TutoTOONS
{
    public class AdService
    {
        public const int STATE_DISABLED = 0;
        public const int STATE_ENABLED = 1;
        public const int STATE_SHOWING_AD = 2;

        public const int SERVICE_CHARTBOOST     = 1;
        public const int SERVICE_INMOBI         = 2;
        public const int SERVICE_SUPER_AWESOME  = 3;
        public const int SERVICE_KIDOZ          = 4;
        public const int SERVICE_AD_MOB         = 5;
        public const int SERVICE_APP_LOVIN      = 6;
        public const int SERVICE_IRONSOURCE     = 7;
        public const int SERVICE_FYBER          = 8;
        public const int SERVICE_AERSERV        = 9;
        public const int SERVICE_AD_COLONY      = 10;
        public const int SERVICE_HEYZAP         = 11;
        public const int SERVICE_OGURY          = 12;
        public const int SERVICE_FLYMOB         = 13;
        public const int SERVICE_REVMOB         = 14;
        public const int SERVICE_VUNGLE         = 15;
        public const int SERVICE_UNITY          = 16;
        public const int SERVICE_TUTOTOONS      = 17;
        public const int SERVICE_POPJAM         = 18;
        public const int SERVICE_PLAYABLE       = 19;
        public const int SERVICE_STARTAPP       = 20;
        public const int SERVICE_HUAWEI         = 21;
        public const int SERVICE_TEST_ADS = 100;

        public const string EVENT_INITIALIZED = "initialized";
        public const string EVENT_REQUEST = "request";
        public const string EVENT_LOADED = "load";
        public const string EVENT_SHOW = "show";
        public const string EVENT_CLICK = "click";
        public const string EVENT_FINISHED = "finished";

        public const double AD_FAILED_TIMEOUT_ANDROID = 5.0;
        public const double AD_FAILED_TIMEOUT_IOS = 5.0;

        public int state;
        public string title;
        public int service_id;
        public List<AdData> ads;
        public AdLocation location_showing;
        public AdData ad_showing;
        public bool ad_started;
        public bool ad_completed;
        public double ad_failed_timeout;
        public int max_interstitial_attempts;
        public int max_interstitial_video_attempts;
        public int max_rewarded_video_attempts;
        public int max_panel_attempts;
        public int max_banner_attempts;
        public bool test_mode;
        public DateTime ad_show_start;

        public AdService()
        {
            state = STATE_DISABLED;
            ads = new List<AdData>();
            ad_failed_timeout = 0.0;

            LogSDKVersions();
        }

        public virtual void SetData(AppConfigAdNetwork _data)
        {
            title = _data.title;
            max_interstitial_attempts = _data.settings.max_interstitial_attempts;
            max_interstitial_video_attempts = _data.settings.max_interstitial_video_attempts;
            max_rewarded_video_attempts = _data.settings.max_rewarded_video_attempts;
            max_panel_attempts = _data.settings.max_panel_attempts;
            max_banner_attempts = _data.settings.max_banner_attempts;
            test_mode = _data.settings.test_mode > 0;
        }

        public virtual void LogSDKVersions()
        {
            //
        }

        public virtual void OnApplicationPause(bool _paused)
        {
            
        }

        public virtual string[] GetVersions()
        {
            return new string[0];
        }

        public virtual void Init()
        {
            //
        }

        public virtual bool LoadAd(AdData _ad, AdPreferences _ad_load_preferences)
        {
            return true;
        }

        public void UpdateSessionAdStats(string _event_type, string _ad_type)
        {
            if (_event_type != EVENT_SHOW)
            {
                return;
            }

            if (AdData.TYPE_INTERSTITIAL == _ad_type)
            {
                AdServices.interstitials_shown++;
            }

            if (AdData.TYPE_INTERSTITIAL_VIDEO == _ad_type)
            {
                AdServices.interstitial_videos_shown++;
            }

            if (AdData.TYPE_VIDEO == _ad_type)
            {
                AdServices.rewarded_videos_shown++;
            }

            if(AdData.TYPE_VIDEO == _ad_type || AdData.TYPE_INTERSTITIAL_VIDEO == _ad_type || AdData.TYPE_INTERSTITIAL == _ad_type)
            {
                AdServices.total_shown++;
            }
        }

        public void SetAdLoaded(string _key, string _type)
        {
            for (int i = 0; i < ads.Count; i++)
            {
                if ((ads[i].key == _key || _key == null) && ads[i].type == _type)
                {
                    ads[i].SetLoaded();
                }
            }
        }

        public void SetAdEmpty(string _key, string _type)
        {
            for (int i = 0; i < ads.Count; i++)
            {
                if ((ads[i].key == _key || _key == null) && ads[i].type == _type)
                {
                    ads[i].SetEmpty();
                }
            }
        }
        
        public void SetAdShown(string _key, string _type)
        {
            //
        }

        public void SetAdFailedToLoad(string _key, string _type)
        {
            for (int i = 0; i < ads.Count; i++)
            {
                if ((ads[i].key == _key || _key == null) && ads[i].type == _type)
                {
                    ads[i].SetFailedToLoad();
                }
            }
        }

        public int GetAdState(string _key, string _type)
        {
            for (int i = 0; i < ads.Count; i++)
            {
                if ((ads[i].key == _key || _key == null) && ads[i].type == _type)
                {
                    return ads[i].state;
                }
            }
            return -1;
        }
        
        public virtual void HidePanelAd(bool show_animation)
        {
            //
        }

        public virtual void HideBannerAd()
        {
            //
        }

        public void TrackEvent(string _event_type, AdLocation _location, string _ad_type)
        {
            if (_event_type == EVENT_REQUEST && !AppConfig.settings.ad_requests_tracker_enabled)
            {
                return;
            }

            if (_event_type == null || title == null || _ad_type == null)
            {
                return;
            }

            UpdateSessionAdStats(_event_type, _ad_type);
            AnalyticsHelper.TrackAdImpressions(title, _event_type, _location, _ad_type);
            string _url = AppConfig.settings.ad_tracker_link + "?action=" + _event_type;
            _url += "&ad_network=" + title;
            _url += "&ad_type=" + _ad_type;

            if(_event_type == EVENT_SHOW)
            {
                if (AdData.TYPE_VIDEO == _ad_type || AdData.TYPE_INTERSTITIAL_VIDEO == _ad_type || AdData.TYPE_INTERSTITIAL == _ad_type)
                {
                    _url += "&ad_count=" + AdServices.total_shown;
                }
                else
                {
                    _url += "&ad_count=0";
                }
            }

            if (_location != null)
            {
                _url += "&ad_id=" + _location.ad_id + "&ad_key=" + _location.ad.key + "&ad_location=" + _location.keyword.getKeyword() + "&ad_campaign_id=" + _location.campaign_id;
            }

            //Debug.Log("url:" + (_location != null) + ": " + _url);

            StatsTracker.CallUrl(_url);
        }

        public void AdStarted()
        {
            ad_started = true;
        }

        public void AdCompleted(bool _completed)
        {
            double _ad_shown_duration = DateTime.Now.Subtract(ad_show_start).TotalSeconds;
            if (AppConfig.settings.ad_reward_time < _ad_shown_duration)
            {
                ad_completed = true;
            }
            else
            {
                ad_completed = _completed;
            }
            state = STATE_ENABLED;

            if (DebugConsole.instance != null && title != null && ad_showing != null && ad_showing.type != null)
            {
                DebugConsole.instance.UpdateLastAdInfo(title, ad_showing.type, ad_completed);
            }
        }

        public bool CanShowAd(AdData _ad)
        {
            if (state != STATE_ENABLED)
            {
                return false;
            }

            return true;
        }

        public virtual bool ShowAd(AdData _ad)
        {
            ad_show_start = DateTime.Now;
            ad_showing = _ad;
            location_showing = _ad.ad_location;
            return true;
        }

        public virtual void Update()
        {
            #if UNITY_ANDROID
            if(state == STATE_SHOWING_AD)
            {
                ad_failed_timeout -= Time.unscaledDeltaTime;

                if(ad_failed_timeout <= 0.0)
                {
                    LogerthaWrapper.LogError(LogerthaWrapper.LOG_CATEGORY_ADS, LogerthaWrapper.GetAdNetworkObjectByServiceID(service_id), LogerthaWrapper.LOG_TYPE_ADS_REMOVED_SHOW_LOCK, "");
                    AdCompleted(true);
                }
            }
            else if(state == STATE_ENABLED)
            {
                ad_failed_timeout = AD_FAILED_TIMEOUT_ANDROID;
            }
            #endif

        }
    }
}