using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TutoTOONS.Utils.Debug.Console
{
    public class SDKDebuggingEntry : MonoBehaviour
    {
        public static List<SDKDebuggingEntry> instances = new List<SDKDebuggingEntry>();
        public Text textTitle;
        public Text textButton;
        public UnityEngine.UI.Button btn;
        public Image img;
        public int filter;
        public bool isEnabled;

        public void Init(int _filter)
        {
            string _name = System.Enum.GetName(typeof(SDKDebug.ServiceName), _filter);
            instances.Add(this);
            filter = _filter;
            textTitle.text = _name;

            isEnabled = SavedData.GetInt("SDKDebugging_" + _name + "_enabled") == 1;
            textButton.text = isEnabled ? "Enabled" : "Disabled";
            img.color = isEnabled ? Color.green : Color.red;

            btn.onClick.AddListener(delegate
            {
                isEnabled = !isEnabled;
                SavedData.SetInt("SDKDebugging_" + _name + "_enabled", isEnabled ? 1 : 0);
                textButton.text = isEnabled ? "Enabled" : "Disabled";
                img.color = isEnabled ? Color.green : Color.red;
                for (int i = 0; i < SDKDebuggingLogEntry.instances.Count; i++)
                {
                    if (SDKDebuggingLogEntry.instances[i] == null) continue;
                    if (SDKDebuggingLogEntry.instances[i].filter == filter)
                    {
                        SDKDebuggingLogEntry.instances[i].gameObject.SetActive(isEnabled);
                    }
                }
            });
        }
    }
}