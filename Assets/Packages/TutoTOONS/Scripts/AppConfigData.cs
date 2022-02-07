using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    [System.Serializable]
    public class AppConfigData
    {
        public AppConfigInfo info;
        public List<AppConfigLocation> ads;
        public AppConfigSettings settings;
        public List<AppConfigAdNetwork> ad_networks;
    }

    [System.Serializable]
    public class AppConfigInfo
    {
        public string bundle_id;
        public string title;
        public string updated;
        public string updated_readable;
        public string production_app_id;
        public string app_version;
        public string platform;
        public string country;
    }

    [System.Serializable]
    public class AppConfigLocation
    {
        public string keyword;
        public int location_id;
        public List<AppConfigAd> ad;
    }

    [System.Serializable]
    public class AppConfigAd
    {
        public int campaign_id;
        public int ad_id;
        public int ad_network_id;
        public int fill_rate;
        public int priority;
        public bool enabled;
        public string key;
        public string key_no_comp;
        public string type;
    }

    [System.Serializable]
    public class AppConfigSettings
    {
        public List<AppConfigAdPriorities> ad_priorities;
        public AppConfigABTests ab_tests;
        public string adjust_app_token;
        public bool tuto_ads_debug_enabled;
        public int tuto_ads_cache_idle_timeout;
        public int tuto_ads_cache_failed_timeout;
        public int tuto_ads_cache_handle_timeout;
        public int tuto_ads_connection_speed_limit;
        public int tuto_ads_connection_timeout;
        public string tuto_ads_speed_test_packet_url;
        public int tuto_ads_speed_test_packet_size;
        public string tuto_ads_vast_url;
        public int tuto_ads_cache_static_limit;
        public int tuto_ads_cache_video_limit;
        public int tuto_ads_cache_panel_limit;
        public bool tuto_ads_auto_cache_static_enabled;
        public bool tuto_ads_auto_cache_video_enabled;
        public bool tuto_ads_auto_cache_panel_enabled;
        public string tuto_ads_install_tracking_url;
        public bool tuto_ads_closable_video_enabled;
        public int tuto_ads_video_interstitial_duration;
        public bool tuto_ads_capping_campaign_capping_enabled;
        public bool tuto_ads_capping_creative_capping_enabled;
        public bool tuto_ads_capping_game_capping_enabled;
        public bool tuto_ads_campaigns_in_a_row_capping_enabled;
        public bool tuto_ads_capping_creative_in_a_row_capping_enabled;
        public bool tuto_ads_capping_game_in_a_row_capping_enabled;
        public bool tuto_ads_capping_max_campaigns_per_session_capping_enabled;
        public bool tuto_ads_capping_max_creatives_per_session_capping_enabled;
        public bool tuto_ads_capping_max_game_per_session_capping_enabled;
        public bool tuto_ads_capping_campaigns_capped_by_ad_type;
        public bool tuto_ads_capping_creatives_capped_by_ad_type;
        public bool tuto_ads_capping_games_by_ad_type;
        public int tuto_ads_capping_limit_campaigns_in_a_row;
        public int tuto_ads_capping_limit_creatives_in_a_row;
        public int tuto_ads_capping_limit_games_in_row;
        public int tuto_ads_capping_by_campaign_limit;
        public int tuto_ads_capping_by_creative_limit;
        public int tuto_ads_capping_by_game_limit;
        public string tuto_ads_shared_data_suite_name;
        public bool tuto_ads_singular_enabled;
        public bool ads_enabled;
        public bool ad_tracker_enabled;
        public string ad_tracker_link;
        public int branch_lock_with_video_ad_timeout;
        public bool branch_tracker_enabled;
        public int branch_video_ad_count;
        public bool chartboost_restrict_data_collection;
        public bool disable_external_ads;
        public int first_ad_delay;
        public int first_banner_delay;
        public int forced_video_ad_interval;
        public string gift_box_item_frequent;
        public string gift_box_item_infrequent;
        public string gift_box_item_special;
        public string gift_box_item_very_rare;
        public string kidoz_app_token;
        public bool kidoz_enabled;
        public int kidoz_publisher_id;
        public bool more_apps_enabled;
        public int next_ad_delay;
        public int next_banner_delay;
        public int ownership;
        public bool players_tracker_enabled;
        public string privacy_policy_link;
        public bool promo_tracker_enabled;
        public int rate_dialog_interval;
        public int rewarded_video_reward;
        public bool show_privacy_policy;
        public int show_rate_us;
        public bool stats_tracker_enabled;
        public string target_age;
        public string target_gender;
        public bool tracker_enabled;
        public string tracker_link;
        public int unlock_all_video_ad_count;
        public string app_key;
        public string flurry_app_key;
        public string flurry_id;
        public string ga_id;
        public double ga_track_amount;
        public string global_ga_id;
        public string push_notifications;
        public bool override_privacy_comp;
        public bool override_data_settings;
        public bool override_age_settings;
        public bool iap_enabled;
        public bool rewarded_ads_enabled;
        public bool interstitial_ads_enabled;
        public string subscription_data_url;
        public string subscription_test_data_url;
        public int subscription_device_limit;
        public string subscription_landing_page_link;
        public bool ad_requests_tracker_enabled;
        public string singular_api_key;
        public string singular_api_secret;
        public string soomla_app_key;
        public string safedk_id;
        public string safedk_key;
        public bool logertha_enabled;
        public string logertha_url;
        public int logertha_max_events;
        public bool super_awesome_skipable_ads;
        public double logertha_init_amount;
        public string fb_app_id;
        public string app_version_live;
        public string update_image_live;
        public float ad_reward_time = 10;
        public int sk_ad_network_controller_type;
        public int sk_ad_network_controller_measurement_period;

        public AppConfigSettings()
        {
            //Setting default values for optional parameters
            ga_track_amount = 1.0;
        }
    }

    [System.Serializable]
    public class AppConfigAdPriorities
    {
        public string type;
        public int campaign_id;
        public string campaign_title;
        public int campaign_priority;
        public List<int> ad_networks;
    }

    [System.Serializable]
    public class AppConfigAdNetwork
    {
        public int ad_network_id;
        public string keyword;
        public string title;
        public AppConfigAdNetworkSettings settings;
    }

    [System.Serializable]
    public class AppConfigAdNetworkSettings
    {
        public bool enabled;
        public string key1;
        public string key2;
        public string key3;
        public string key1_no_comp;
        public string key2_no_comp;
        public string key3_no_comp;
        public int test_mode;
        public bool allow_no_comp;
        public int max_interstitial_attempts = -1;
        public int max_interstitial_video_attempts = -1;
        public int max_rewarded_video_attempts = -1;
        public int max_panel_attempts = -1;
        public int max_banner_attempts = -1;
    }

    [System.Serializable]
    public class AppConfigABTests
    {
        public string enabled_countries;
        public List<AppConfigABTest> tests;
    }

    [System.Serializable]
    public class AppConfigABTest
    {
        public int test_id;
        public double percentage;
        public List<AppConfigABTestVariant> variants;
    }

    [System.Serializable]
    public class AppConfigABTestVariant
    {
        public int variant_id;
        public string parameters;
    }
}
