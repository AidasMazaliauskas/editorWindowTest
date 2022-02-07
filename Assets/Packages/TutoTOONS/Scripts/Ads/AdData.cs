using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    public class AdData
    {
        public const int DEFAULT_TIMEOUT_LENGHT = 3;

        public const string TYPE_INTERSTITIAL = "interstitial";
        public const string TYPE_INTERSTITIAL_VIDEO = "interstitial_video";
        public const string TYPE_STATIC = "static";
        public const string TYPE_VIDEO = "video";
        public const string TYPE_BANNER = "banner";
        public const string TYPE_MORE_APPS = "more_apps";
        public const string TYPE_PANEL = "panel";
        public const string TYPE_PLAYABLE = "playable";

        public const int STATE_EMPTY = 0;
        public const int STATE_LOADING = 1;
        public const int STATE_LOADED = 2;
        public int state;
        public int ad_network_id;
        public AdLocation ad_location;
        public string key;
        public string key_no_comp;
        public string type;
        public double preload_timeout;
        public double start_load_time;
        public AdService ad_service;

        public int max_attempts;
        public int attempt;

        public AdData(int _ad_network_id, string _key, string _key_no_comp, string _type, AdLocation _ad_location)
        {
            ad_network_id = _ad_network_id;
            key = _key;
            key_no_comp = _key_no_comp;
            type = _type;
            ad_location = _ad_location;

            state = STATE_EMPTY;
            preload_timeout = 0.0;
            attempt = 0;
            max_attempts = -1;
        }

        public void SetAdService(AdService _ad_service)
        {
            ad_service = _ad_service;
            SetMaxAttempts();
        }

        public void SetStartedLoading()
        {
            start_load_time = Time.unscaledTime;
            state = STATE_LOADING;
            preload_timeout = 10;
        }

        public void SetLoaded()
        {
            double _load_time = Time.unscaledTime - start_load_time;
            LogerthaWrapper.Log(LogerthaWrapper.LOG_CATEGORY_ADS, LogerthaWrapper.GetAdNetworkObjectByServiceID(ad_network_id), LogerthaWrapper.LOG_TYPE_AD_LOADED, type, _load_time);

            state = STATE_LOADED;
            attempt = -1;
        }

        public void SetEmpty()
        {
            state = STATE_EMPTY;
            attempt++;
            preload_timeout = GetTimeoutLenght(attempt);
        }

        public void SetFailedToLoad()
        {
            state = STATE_EMPTY;
            attempt++;
            preload_timeout = GetTimeoutLenght(attempt);
        }

        public bool CanLoadAd()
        {
            if (max_attempts < 0)
            {
                return true;
            }
            else if (attempt < max_attempts)
            {
                return true;
            }

            return false;
        }

        public void UpdateAdLocation(AdLocation _ad_location)
        {
            ad_location = _ad_location;
        }

        private int GetTimeoutLenght(int _iteration)
        {
            int _timeout = 0;

            for (int i = 0; i < _iteration; i++)
            {
                if (_timeout == 0)
                {
                    _timeout = DEFAULT_TIMEOUT_LENGHT;
                }
                else
                {
                    _timeout *= 2;
                }
            }

            return _timeout;
        }

        private void SetMaxAttempts()
        {
            if (type == TYPE_INTERSTITIAL)
            {
                max_attempts = ad_service.max_interstitial_attempts;
            }
            else if (type == TYPE_INTERSTITIAL_VIDEO)
            {
                max_attempts = ad_service.max_interstitial_video_attempts;
            }
            else if (type == TYPE_VIDEO)
            {
                max_attempts = ad_service.max_rewarded_video_attempts;
            }
            else if (type == TYPE_PANEL)
            {
                max_attempts = ad_service.max_panel_attempts;
            }
            else if (type == TYPE_BANNER)
            {
                max_attempts = ad_service.max_banner_attempts;
            }
        }
    }
}