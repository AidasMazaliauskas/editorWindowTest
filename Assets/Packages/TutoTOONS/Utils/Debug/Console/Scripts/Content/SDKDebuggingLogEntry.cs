using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS.Utils.Debug.Console
{
    public class SDKDebuggingLogEntry : MonoBehaviour
    {
        public static List<SDKDebuggingLogEntry> instances = new List<SDKDebuggingLogEntry>();
        public int filter;

        public void Init(int _filter)
        {
            filter = _filter;
            bool _found = false;
            for (int i = 0; i < SDKDebuggingEntry.instances.Count; i++)
            {
                if (SDKDebuggingEntry.instances[i] == null) continue;
                if (SDKDebuggingEntry.instances[i].filter == filter)
                {
                    gameObject.SetActive(SDKDebuggingEntry.instances[i].isEnabled);
                    _found = true;
                }
            }
            if (!_found) gameObject.SetActive(false);
            instances.Add(this);
        }

        void OnDestroy()
        {
            instances.Remove(this);
        }
    }
}