using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace TutoTOONS.Utils.Debug.Console
{
    public class ContentSDKDebugging : MonoBehaviour
    {
        public static ContentSDKDebugging instance;
        public GameObject prefabEntry;
        public Transform tList;
        public Text textLoading;
        public static List<SDKLog> logQueue = new List<SDKLog>();
        public static List<SDKDebug.ServiceName> activated_debug_services = new List<SDKDebug.ServiceName>();
        public ScrollRect scroll_rect;

        private void OnEnable()
        {
            scroll_rect.horizontalNormalizedPosition = 0f;
        }

        public void Init()
        {
            if (instance == null)
            {
                instance = this;
                GenerateEntries();
            }
            else
            {
                Destroy(this);
            }
        }

        public static void InitDebugService(SDKDebug.ServiceName _service_name)
        {
            if (instance != null)
            {
                instance.InitDebugEntry(_service_name);
            }

            activated_debug_services.Add(_service_name);
        }

        public void InitDebugEntry(SDKDebug.ServiceName _service_name)
        {
            int _value = (int)Enum.Parse(typeof(SDKDebug.ServiceName), Enum.GetName(typeof(SDKDebug.ServiceName), _service_name));
            SDKDebuggingEntry _debug_service = Instantiate(prefabEntry, tList).GetComponent<SDKDebuggingEntry>();
            _debug_service.Init(_value);
        }

        public void GenerateEntries()
        {
            foreach (SDKDebug.ServiceName _service in activated_debug_services)
            {
                InitDebugEntry(_service);
            }
        }
    }
}