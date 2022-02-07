using TutoTOONS.Utils.Debug.Console;
using UnityEngine;
using TutoTOONS;

namespace TutoTOONS.Utils.Debug
{ 
    public class SDKDebug
    {
        public enum ServiceName
        {
            TutoAds,
            Chartboost,
            SuperAwesome,
            Kidoz,
            IronSourceMediation,
            AdMobMediation,
            ApplovinMAX,
            StatsTracker,
            HuaweiAds,
            Subscription,
            TutoTools,
            HuaweiIAP
        }

        public static void InitDebugService(ServiceName _service_name)
        {
            ContentSDKDebugging.InitDebugService(_service_name);
        }
        
        public static void Log(ServiceName _service_name, string _message)
        {
            ContentSDKDebugging.logQueue.Insert(0, new SDKLog(_service_name, _message));
        }

        public static bool IsServiceInTestMode(ServiceName _service_name)
        {
            if(SavedData.GetInt("SDKDebugging_" + System.Enum.GetName(typeof(ServiceName), _service_name) + "_enabled", 0) == 1)
            {
                return true;
            }

            return false;
        }
    }
}