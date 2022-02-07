using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
Custom metrics, max 20:
cd2=custom_metric_2
cm2=metric_2_value

Sample URL:
https://www.google-analytics.com/collect?v=1&ds=app&tid=UA-65233426-47&t=screenview&cid=936GRX8MEJ3UQ5AO&cd=Loader&an=app&av=0.0.1&ua=TutoTOONS/1 (Android)&sr=1920x1080&vp=1920x1080&qt=852

*/

namespace TutoTOONS
{
    public class GoogleAnalytics
    {
        public static double time_from_last_call;

        const int STATE_DISABLED = 0;
        const int STATE_INITIALIZING = 1;
        const int STATE_ENABLED = 2;
        const int STATE_SKIPPED = 3;
        static int state = STATE_DISABLED;
        const string URL_BASE = "https://www.google-analytics.com/collect?v=1&ds=app&tid=";

        const string GA_TRACK_AMOUNT = "google_analytics_track_amount";
        const string GA_TRACK_ENABLED = "google_analytics_tracking_enabled";
        static string ga_id = "*ga_id*";
        static List<URLStackItem> url_stack;
        static bool session_started;
        static double device_track_amount;

        public static void Init()
        {
            if (SavedData.HasKey(GA_TRACK_AMOUNT))
            {
                device_track_amount = SavedData.GetDouble(GA_TRACK_AMOUNT);
            }
            else
            {
                System.Random rand_gen = new System.Random();
                device_track_amount = rand_gen.NextDouble();
                SavedData.SetDouble(GA_TRACK_AMOUNT, device_track_amount);
                SavedData.SetInt(GA_TRACK_ENABLED, 1);
                Debug.Log("Initializing google analytics with track amount " + device_track_amount);
            }
            state = STATE_INITIALIZING;
            session_started = false;

            time_from_last_call = 0.0;
        }

        public static void TrackPageview(string _page)
        {
            if (state == STATE_INITIALIZING || state == STATE_ENABLED)
            {
                string _params = "cd=" + _page;
                _params += "&an=app";
                if (state == STATE_ENABLED)
                {
                    _params += "&av=" + Application.version;
                }
                else
                {
                    _params += "&av=*app_version*";
                }
                SendData("screenview", _params);

                /* Possible parameters:
				&an=funTimes                // App name.
				&av=1.5.0                   // App version.
				&aid=com.foo.App            // App Id.
				&aiid=com.android.vending   // App Installer Id.
				&cd=Home                    // Screen name / content description.
				*/
            }
        }

        public static void TrackEvent(string _category, string _action = null, string _label = null, int _value = int.MinValue)
        {
            if (state == STATE_INITIALIZING || state == STATE_ENABLED)
            {
                string _params = "ec=" + _category;
                if (_action != null)
                {
                    _params += "&ea=" + _action;
                }
                if (_label != null)
                {
                    _params += "&el=" + _label;
                }
                if (_value != int.MinValue)
                {
                    _params += "&ev=" + _value;
                }
                SendData("event", _params);
            }
        }

        public static void TrackGlobalSession()
        {
            //Track session to global Google Analytics ID (TutoApp v3)
            //Only tracked if GA ID is assigned in config.
            //Default global ID: UA-72644219-33

            if (AppConfig.settings.global_ga_id == null)
            {
                return;
            }
            if (AppConfig.settings.global_ga_id.Length == 0)
            {
                return;
            }

            string GA_URL = "https://www.google-analytics.com/collect?v=1&ds=app&tid=" + AppConfig.settings.global_ga_id + "&ds=app&t=event&cid=" + SavedData.player_id + "&ec=App&ea=Version&el=Unity_" + AppConfig.PACKAGE_VERSION;
            //GA_URL += "&ua=" + GoogleAnalytics.GenerateUserAgent();
            GA_URL += "&an=" + SystemUtils.GetBundleID();
            GA_URL += "&sr=" + Screen.currentResolution.width + "x" + Screen.currentResolution.height;
            UnityWebRequest GA_loader = UnityWebRequest.Get(GA_URL);
            GA_loader.SendWebRequest();
            /* Other option is to use 'screenview' type
			&t=screenview               // Screenview hit type.
			&an=funTimes                // App name.
			&av=1.5.0                   // App version.
			&aid=com.foo.App            // App Id.
			&aiid=com.android.vending   // App Installer Id.
			&cd=Home                    // Screen name / content description.
			*/
        }

        static void SendData(string _type, string _params)
        {
            string ga_url = URL_BASE + ga_id + "&t=" + _type + "&cid=" + SavedData.player_id + "&" + _params;
            //ga_url += "&ua=" + SystemInfo.deviceModel; Old tracking
            ga_url += "&ua=" + GenerateUserAgent();
            string screen_size = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
            ga_url += "&sr=" + screen_size;
            ga_url += "&vp=" + screen_size;

            if (!session_started)
            {
                // Set forced session start. If player closes app and then launches again after few minutes,
                // GA would track it as one continuous session if not sending forced start.
                session_started = true;
                //Session control disabled from 2019-02-12, to make GA work as in Builder games.
                //ga_url += "&sc=start";
            }
            if (state == STATE_ENABLED)
            {
                CallURL(ga_url);
            }
            else if (state == STATE_INITIALIZING)
            {
                //Not initialized yet, add to stack and send later.
                //Debug.Log("Adding to stack");
                if (url_stack == null)
                {
                    url_stack = new List<URLStackItem>();
                }
                // Second parameter (0) is hit delay between when the hit being reported occurred and the time the hit was sent, in milliseconds.
                url_stack.Add(new URLStackItem(ga_url));
            }
        }

        static void CallURL(string _url)
        {
            if (ABTesting.state == ABTesting.STATE_ENABLED)
            {
                //A/B testing seems to be removed from Google Analytics REST specification
                _url += "&xid=" + ABTesting.test_id;
                _url += "&xvar=" + ABTesting.variant_id;
            }
            //Debug.Log("Tracking: " + _url);
            UnityWebRequest ga_loader = UnityWebRequest.Get(_url);
            //ga_loader.SetRequestHeader("User-Agent", GenerateUserAgent());
            ga_loader.SendWebRequest();
            time_from_last_call = 0.0;
        }

        public static string GenerateUserAgent()
        {
            //return "TutoTOONS/" + AppConfig.version + " (" + AppConfig.platform_name + ")";
#if UNITY_IOS
            //iPhone; CPU iPhone OS
            return "TutoTOONS/" + AppConfig.version + " (iOS; " + SystemInfo.deviceModel + ")"; //iOS_device_page
#else
            return "TutoTOONS/" + AppConfig.version + " (Android; " + SystemInfo.deviceModel + ")";
#endif
        }

        public static void Update()
        {
            time_from_last_call += Time.unscaledDeltaTime;
            if (state == STATE_INITIALIZING)
            {
                if (url_stack != null)
                {
                    for (int i = 0; i < url_stack.Count; i++)
                    {
                        url_stack[i].time += Time.unscaledDeltaTime * 1000;
                    }
                }
                if (AppConfig.state == AppConfig.STATE_LOADED)
                {
                    if (SavedData.first_run)
                    {
                        TrackGlobalSession();
                    }

                    if (SavedData.GetInt(GA_TRACK_ENABLED) <= 0)
                    {
                        return;
                    }

                    if (device_track_amount <= AppConfig.settings.ga_track_amount)
                    {
                        Debug.Log("Google analytics enabled");
                        if (AppConfig.settings.ga_id == null || AppConfig.settings.ga_id == "")
                        {
                            state = STATE_DISABLED;
                        }
                        else
                        {
                            ga_id = AppConfig.settings.ga_id;
                            //ga_id = "UA-65233426-47";   //For testing
                            state = STATE_ENABLED;
                            if (url_stack != null)
                            {
                                for (int i = 0; i < url_stack.Count; i++)
                                {
                                    string _url = url_stack[i].url.Replace("*ga_id*", ga_id);
                                    _url = _url.Replace("*app_version*", Application.version);
                                    _url += "&qt=" + (int)url_stack[i].time;
                                    CallURL(_url);
                                }
                                url_stack = null;
                            }
                        }
                    }
                    else
                    {
                        state = STATE_SKIPPED;
                        url_stack = null;
                        SavedData.SetInt(GA_TRACK_ENABLED, 0);
                    }
                }
            }
        }
    }

    class URLStackItem
    {
        public string url;
        public double time;

        public URLStackItem(string _url)
        {
            url = _url;
            time = 0.0;
        }
    }
}
