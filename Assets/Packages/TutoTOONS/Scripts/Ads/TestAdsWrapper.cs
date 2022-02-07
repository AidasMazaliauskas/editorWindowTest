using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    public class TestAdsWrapper : AdService
    {
        public const int TRIES_LIMIT = 5;

        private int tries_counter;
        private bool try_show_ad;

        public TestAdsWrapper()
        {
            service_id = SERVICE_TEST_ADS;
        }

        public override void Init()
        {
            TestAdsController.instance.SetCloseCallback(EventAdClosed);
            state = STATE_ENABLED;
        }

        public override bool LoadAd(AdData _ad, AdPreferences _ad_load_preferences)
        {
            if (_ad.state != AdData.STATE_EMPTY)
            {
                return false;
            }
            if (_ad.type == AdData.TYPE_INTERSTITIAL)
            {
                SetAdLoaded(null, AdData.TYPE_INTERSTITIAL);
                return true;
            }
            if (_ad.type == AdData.TYPE_INTERSTITIAL_VIDEO)
            {
                SetAdLoaded(null, AdData.TYPE_INTERSTITIAL_VIDEO);
                return true;
            }
            else if (_ad.type == AdData.TYPE_VIDEO)
            {
                SetAdLoaded(null, AdData.TYPE_VIDEO);
                return true;
            }
            return false;
        }
        
        public override bool ShowAd(AdData _ad)
        {
            base.ShowAd(_ad);
            if (_ad.type == AdData.TYPE_INTERSTITIAL)
            {
                state = STATE_SHOWING_AD;
                try_show_ad = true;
                tries_counter = 0;
                UpdateSessionAdStats(EVENT_SHOW, _ad.type);
                return true;
            }
            if (_ad.type == AdData.TYPE_INTERSTITIAL_VIDEO)
            {
                state = STATE_SHOWING_AD;
                try_show_ad = true;
                tries_counter = 0;
                UpdateSessionAdStats(EVENT_SHOW, _ad.type);
                return true;
            }
            else if (_ad.type == AdData.TYPE_VIDEO)
            {
                state = STATE_SHOWING_AD;
                try_show_ad = true;
                tries_counter = 0;
                UpdateSessionAdStats(EVENT_SHOW, _ad.type);
                return true;
            }
            return false;
        }

        public void EventAdClosed(bool _completed)
        {
            AdCompleted(_completed);
        }

        private void TryShowAd()
        {
            if (location_showing != null)
            {
                try_show_ad = false;
                TestAdsController.instance.ShowAd(location_showing.ad.type, location_showing.keyword.getKeyword());
            }
            else
            {
                tries_counter++;
                if(tries_counter > TRIES_LIMIT)
                {
                    try_show_ad = false;
                    TestAdsController.instance.ShowAd("undefined", "undefined");
                }
            }
        }

        public static AppConfigAdNetwork GetAdnetworkParameters()
        {
            AppConfigAdNetworkSettings _settings = new AppConfigAdNetworkSettings();
            _settings.enabled = true;
            _settings.key1 = "";
            _settings.key2 = "";
            _settings.key3 = "";
            _settings.key1_no_comp = "";
            _settings.key2_no_comp = "";
            _settings.key3_no_comp = "";
            _settings.test_mode = 0;
            _settings.allow_no_comp = true;

            AppConfigAdNetwork _ad_network = new AppConfigAdNetwork();
            _ad_network.ad_network_id = SERVICE_TEST_ADS;
            _ad_network.keyword = "testads";
            _ad_network.title = "Test Ads";
            _ad_network.settings = _settings;

            return _ad_network;
        }

        public override void Update()
        {
           if(try_show_ad)
            {
                TryShowAd();
            }
        }
    }
}
