using UnityEngine;
using UnityEngine.UI;

namespace TutoTOONS.Utils.Debug.Console
{
    public class ContentStats : MonoBehaviour
    {
        public Text text_forced_ads_timeout;
        public Text text_forced_ads_branch_end_timeout;
        public Text text_interstitial_show_counter;
        public Text text_interstitial_video_show_counter;
        public Text text_rewarded_show_counter;
        public Text text_total_ads_shown_counter;
        public ScrollRect scroll_rect;

        void Start()
        {
            text_forced_ads_timeout.text = "Forced Ads Timeout: AdServices Not Initialized";
            text_forced_ads_branch_end_timeout.text = "Forced Ads Branch End Timeout: AdServices Not Initialized";
            text_interstitial_show_counter.text = "Interstitials Shown: AdServices Not Initialized";
            text_interstitial_video_show_counter.text = "Interstitial Videos Shown: AdServices Not Initialized";
            text_rewarded_show_counter.text = "Rewarded Videos Shown: AdServices Not Initialized";
            text_total_ads_shown_counter.text = "Total Ads Shown: AdServices Not Initialized";
        }

        private void OnEnable()
        {
            scroll_rect.horizontalNormalizedPosition = 0f;
        }

        public void UpdateAdStats()
        {
            if (AdServices.forced_ads_timeout > 0)
            {
                text_forced_ads_timeout.text = "Forced Ads Timeout: " + AdServices.forced_ads_timeout.ToString(".##");
            }
            else
            {
                text_forced_ads_timeout.text = "Forced Ads Timeout: Ready To Show";
            }

            if (AdServices.forced_location_branch_end_timeout > 0)
            {
                text_forced_ads_branch_end_timeout.text = "Forced Ads Branch End Timeout: " + AdServices.forced_location_branch_end_timeout.ToString(".##");
            }
            else
            {
                text_forced_ads_branch_end_timeout.text = "Forced Ads Branch End Timeout: Ready To Show";
            }

            text_interstitial_show_counter.text = "Interstitials Shown: " + AdServices.interstitials_shown;
            text_interstitial_video_show_counter.text = "Interstitial Videos Shown: " + AdServices.interstitial_videos_shown;
            text_rewarded_show_counter.text = "Rewarded Videos Shown: " + AdServices.rewarded_videos_shown;
            text_total_ads_shown_counter.text = "Total Ads Shown: " + AdServices.total_shown;
        }

        void Update()
        {
            switch (AppConfig.state)
            {
                case AppConfig.STATE_LOADED:
                    if (AdServices.state == AdServices.STATE_ENABLED)
                    {
                        UpdateAdStats();
                    }
                    break;
            }
        }
    }
}
