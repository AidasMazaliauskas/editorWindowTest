using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace TutoTOONS
{
    public class PrivacyPolicyController
    {
        private static InAppBrowserBridge _bridge;
        private static InAppBrowserBridge bridge { 
            get
            {
                if (_bridge == null)
                {
                    _bridge = GameObject.FindObjectOfType<InAppBrowserBridge>();
                }
                return _bridge;
            }
        }

        public static void Show()
        {
            InAppBrowser.DisplayOptions _display_options = new InAppBrowser.DisplayOptions();
            _display_options.insets = GetWebViewPadding();
            _display_options.hidesHistoryButtons = true;


            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                // Device is conected to the Internet
                string _url = GetPrivacyPolicyURL();
                InAppBrowser.OpenURL(_url, _display_options);
            }
            else
            {
                bridge.onBrowserFinishedLoadingWithError.RemoveAllListeners();
                bridge.onBrowserFinishedLoadingWithError.AddListener(OnBrowserFinishedLoadingWithError);
                InAppBrowser.OpenLocalFile("PrivacyPolicy/PrivacyPolicyNoWifi.html", _display_options);
            }
        }

        private static void OnBrowserFinishedLoadingWithError(string _url, string _error)
        {           
            if (_error == "net::ERR_INTERNET_DISCONNECTED")
            {
                InAppBrowser.CloseBrowser();
                InAppBrowser.DisplayOptions _display_options = new InAppBrowser.DisplayOptions();
                _display_options.insets = GetWebViewPadding();
                _display_options.hidesHistoryButtons = true;
                InAppBrowser.OpenLocalFile("PrivacyPolicy/PrivacyPolicyNoWifi.html", _display_options);
            }
        }
        
        private static string GetPrivacyPolicyURL()
        {
            string _privacy_policy_url = null;

            if (AppConfig.settings != null)
            {
                _privacy_policy_url = AppConfig.settings.privacy_policy_link;
            }

            if (string.IsNullOrEmpty(_privacy_policy_url))
            {
                switch (AppConfig.account)
                {
                    case (AppConfig.ACCOUNT_TUTOTOONS):
                        _privacy_policy_url = "https://tutotoons.com/privacy_policy/embeded";
                        break;

                    case AppConfig.ACCOUNT_CUTE_AND_TINY:
                        _privacy_policy_url = "https://cutetinygames.com/privacy_policy/embedded";
                        break;

                    case AppConfig.ACCOUNT_SPINMASTER:
                        _privacy_policy_url = "https://spinmaster.helpshift.com/a/mightyexpress/?p=web&s=privacy-policy&f=spin-master-privacy-policy&l=en";
                        break;

                    case AppConfig.ACCOUNT_SUGARFREE:
                        _privacy_policy_url = "https://sugarfree.games/privacy.html";
                        break;

                    default:
                        // TODO: Figure out which privacy policy should be shown here
                        _privacy_policy_url = "https://tutotoons.com/privacy_policy/embeded";
                        break;
                }
            }
            return _privacy_policy_url;
        }

        private static InAppBrowser.EdgeInsets GetWebViewPadding()
        {
            int _padding_top = 0;
            int _padding_bottom = 0;
            int _padding_left = 0;
            int _padding_right = 0;

            return new InAppBrowser.EdgeInsets(_padding_top, _padding_bottom, _padding_left, _padding_right);
        }
    }
}
