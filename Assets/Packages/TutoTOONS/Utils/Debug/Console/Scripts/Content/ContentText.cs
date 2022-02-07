using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TutoTOONS.Utils.Debug.Console
{
    public class ContentText : MonoBehaviour
    {
        public Transform transform_content_list;
        public GameObject prefab_log_entry;
        public ScrollRect scrollRect;
        public RectTransform rt;
        public Text stop_button_text;
        public Color[] log_colors;
        public int rows_limit = 50;
        public bool isSDKLogs;
        int shouldScrollDown;

        void Update()
        {
            while (transform_content_list.childCount > 0 && transform_content_list.childCount > rows_limit)
            {
                DestroyImmediate(transform_content_list.GetChild(0).gameObject);
            }
            FillLogs();
            if (shouldScrollDown > 0)
            {
                shouldScrollDown--;
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        void FillLogs()
        {
            if (!isSDKLogs) return;

            for (int i = ContentSDKDebugging.logQueue.Count - 1; i >= 0; i--)
            {
                string _text = ContentSDKDebugging.logQueue[i].message + "\n" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\n";
                AddLog(_text, 0, (int)ContentSDKDebugging.logQueue[i].service_name);
                ContentSDKDebugging.logQueue.RemoveAt(i);
            }
        }

        public void ChangePauseState()
        {
            // used only in 'Logs' tab
            DebugConsole.instance.console.logsActive = !DebugConsole.instance.console.logsActive;
            if (stop_button_text != null) {
                stop_button_text.text = DebugConsole.instance.console.logsActive ? "Pause Logs" : "Continue Logs";
            } 
        }

        void OnEnable()
        {
            FillLogs();
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            shouldScrollDown = 3;
            scrollRect.horizontalNormalizedPosition = 0f;
        }

        public void SaveLogs(bool debug_logs)
        {
            string file_name;
            string shareMessage;
            if (debug_logs)
            {
                file_name = "_sdk_logs";
                shareMessage = GetDebugLogsText();
            }
            else
            {
                file_name = "_logs";
                shareMessage = GetLogsText();
            }

            string logName = Application.identifier + "_" + System.DateTime.Now.ToString("HH:mm") + file_name;
            string filePath = Path.Combine(Application.persistentDataPath, "_" + logName + ".txt");
            File.WriteAllText(filePath, shareMessage);

            UnityEngine.Debug.Log("Logs were saved in: " + filePath);
        }

        public void CopyLogs(bool debug_logs)
        {
            if (debug_logs)
            {
                GUIUtility.systemCopyBuffer = GetDebugLogsText();
            }
            else
            {
                GUIUtility.systemCopyBuffer = GetLogsText();
            }

            UnityEngine.Debug.Log("Logs were coppied");
        }

        private string GetDebugLogsText()
        {
            var shareMessage = "";

            for (int i = 0; i < LogEntry.instances.Count; i++)
            {
                if (LogEntry.instances[i].is_debug_log)
                {
                    shareMessage += LogEntry.instances[i].full_text + "\n";
                }

            }

            return shareMessage;
        }

        private string GetLogsText()
        {
#if GEN_SUBSCRIPTION
            DebugConsole.instance.console.HandleLog("SubscriptionDataConfig.isLoaded: " + Subscription.SubscriptionManager.SubscriptionDataConfig.isLoaded, "", LogType.Log);
            DebugConsole.instance.console.HandleLog("SubscriptionDataConfig.subscriptionEnabledOnPlatform: " + Subscription.SubscriptionManager.SubscriptionDataConfig.subscriptionEnabledOnPlatform, "", LogType.Log);
            DebugConsole.instance.console.HandleLog("SubscriptionDataConfig.showPromoPanel: " + Subscription.SubscriptionManager.SubscriptionDataConfig.showPromoPanel, "", LogType.Log);
            DebugConsole.instance.console.HandleLog("SubscriptionDataConfig.playPassEnabled: " + Subscription.SubscriptionManager.SubscriptionDataConfig.playPassEnabled, "", LogType.Log);
            DebugConsole.instance.console.HandleLog("subscription_active: " + SavedData.GetInt("subscription_active"), "", LogType.Log);
#endif
            if (AppConfig.settings != null)
            {
                DebugConsole.instance.console.HandleLog("AppConfig.settings.iap_enabled: " + AppConfig.settings.iap_enabled, "", LogType.Log);
                DebugConsole.instance.console.HandleLog("AppConfig.settings.ads_enabled: " + AppConfig.settings.ads_enabled, "", LogType.Log);
                DebugConsole.instance.console.HandleLog("AppConfig.settings.rewarded_ads_enabled: " + AppConfig.settings.rewarded_ads_enabled, "", LogType.Log);
                DebugConsole.instance.console.HandleLog("AppConfig.settings.interstitial_ads_enabled: " + AppConfig.settings.interstitial_ads_enabled, "", LogType.Log);
            }
            DebugConsole.instance.console.HandleLog("AdServices.state: " + AdServices.state, "", LogType.Log);
#if BUILDER_UNITY
            DebugConsole.instance.console.HandleLog("IAPController.ProductIsPurchased(ProductTypeIAP.UnlockAll): " + IAPController.ProductIsPurchased(ProductTypeIAP.UnlockAll), "", LogType.Log);
            DebugConsole.instance.console.HandleLog("IAPController.ProductIsPurchased(ProductTypeIAP.NoAds): " + IAPController.ProductIsPurchased(ProductTypeIAP.NoAds), "", LogType.Log);
#endif

            var shareMessage = "";

            for (int i = 0; i < LogEntry.instances.Count; i++)
            {
                if (!LogEntry.instances[i].is_debug_log)
                {
                    shareMessage += LogEntry.instances[i].full_text + "\n";
                }
            }

            return shareMessage;
        }

        public void AddLog(string text, int color = 0, int filter = -1)
        {
            var go = Instantiate(prefab_log_entry, transform_content_list);
            var le = go.GetComponent<LogEntry>();
            le.text.text = text;
            le.text.color = log_colors[color];
            if(filter >= 0)
            {
                go.AddComponent<SDKDebuggingLogEntry>().Init(filter);
                le.is_debug_log = true;
            }
            le.Init();

            var thres = 40f / rt.sizeDelta.y;

            if (scrollRect.verticalNormalizedPosition <= thres)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
                shouldScrollDown = 3;
            }
        }
    }
}
