using UnityEngine;
using UnityEngine.Networking;
using TutoTOONS.Utils.Debug;

namespace TutoTOONS
{
    public class StatsTracker
    {
        //public const string AD_TRACKER_URL = "https://tracker.tutotoons.com/ad_tracker.php";  //This is not required. Take from config instead: AppConfig.settings.ad_tracker_link

        public static void CallUrl(string _url)
        {
            #if GEN_FREETIME
            return;
            #endif
            
            if (!AppConfig.settings.ad_tracker_enabled)
            {
                return;
            }
            
            if (_url.IndexOf("&platform=") <= 0)
            {
                _url += "&platform=" + AppConfig.platform_name;
            }
            if (_url.IndexOf("&bundle_id=") <= 0)
            {
                _url += "&bundle_id=" + SystemUtils.GetBundleID();
            }
            if (_url.IndexOf("&app_version=") <= 0)
            {
                _url += "&app_version=" + Application.version;
            }
         
            UnityWebRequest _url_request = UnityWebRequest.Get(_url);
            _url_request.SendWebRequest();

            SDKDebug.Log(SDKDebug.ServiceName.StatsTracker, "StatsTracker CallUrl (url: " + _url + ")");


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
    }
}