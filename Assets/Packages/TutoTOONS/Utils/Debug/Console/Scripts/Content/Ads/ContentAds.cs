using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TutoTOONS;
using System.IO;

namespace TutoTOONS.Utils.Debug.Console
{
    public class ContentAds : MonoBehaviour
    {
        public GameObject ad_service_data_prefab;
        public GameObject loading_app_config_prefab;
        public GameObject scroll_panel;
        public ScrollRect scroll_rect;
        public Text last_ad_info;
        public Text consent_age_info;

        private List<GameObject> ad_service_data = new List<GameObject>();
        private GameObject loading_app_config;
        private bool waiting_for_app_config = true;

        void Start()
        {
            loading_app_config = Instantiate(loading_app_config_prefab, scroll_panel.transform);
            
        }

        public void UpdateLastAdInfo(string _text)
        {
            last_ad_info.text = _text;
        }

        public void UpdateInfo()
        {
            consent_age_info.text = "Age over kids protection limit: " + AdServices.age_over_kids_protection_limit + "\n";
            consent_age_info.text += "Consent to collect data: " + AdServices.consent_to_collect_data;
        }

        private void OnEnable()
        {
            scroll_rect.horizontalNormalizedPosition = 0f;
        }

        private void GenerateAdServicesData()
        {
            if (ad_service_data_prefab != null)
            {
                for (int i = 0; i < AdServices.unique_ads.Count; i++)
                {
                    GameObject _slot = Instantiate(ad_service_data_prefab, scroll_panel.transform);
                    ad_service_data.Add(_slot);
                    _slot.GetComponent<AdServiceData>().Init(AdServices.unique_ads[i]);
                }

                if (ad_service_data.Count == 0)
                {
                    return;
                }
            }
        }

        void Update()
        {
            if (waiting_for_app_config)
            {
                switch (AppConfig.state)
                {
                    case AppConfig.STATE_LOADED:
                        if (AdServices.state == AdServices.STATE_ENABLED)
                        {
                            Destroy(loading_app_config);
                            UpdateInfo();
                            GenerateAdServicesData();                            
                            waiting_for_app_config = false;
                        }
                        else
                        {
                            if (AdServices.state == AdServices.STATE_DISABLED)
                            {
                                loading_app_config.GetComponent<Text>().text = "Ad services are disabled.";
                            }
                            else
                            {
                                loading_app_config.GetComponent<Text>().text = "Loading ad services";
                            }
                        }
                        break;

                    case AppConfig.STATE_LOADING:
                        loading_app_config.GetComponent<Text>().text = "Loading AppConfig...";
                        break;

                    case AppConfig.STATE_FAILED:
                        loading_app_config.GetComponent<Text>().text = "Failed to load AppConfig.";
                        break;

                    case AppConfig.STATE_DISABLED:
                        loading_app_config.GetComponent<Text>().text = "AppConfig is disabled.";

                        #if GEN_FREETIME
                        loading_app_config.GetComponent<Text>().text = "Freetime games doesn't have ads and IAPs.";
                        #endif

                        break;



                }
            }
        }
    }
}
