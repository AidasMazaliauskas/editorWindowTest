using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TutoTOONS.Utils.Debug.Console
{
    public class LogEntry : MonoBehaviour
    {
        public static List<LogEntry> instances = new List<LogEntry>();
        public UnityEngine.UI.Button button;
        public Text copy_button_text;
        public Text text;
        public RectTransform rt;

        public string full_text;
        public bool is_debug_log;
        private string short_text;
        private bool show_full;
        private bool update_size = true;
        private float coppy_button_timer = -1;

        void OnDestroy()
        {
            instances.Remove(this);
        }

        public void Init()
        {
            instances.Add(this);

            full_text = text.text;
            short_text = full_text;

            var split = text.text.Split('\n');
            if (split.Length > 0)
            {
                short_text = split[0];
            }

            if (short_text.Length > 100)
            {
                short_text = short_text.Substring(0, 100) + "...";
            }

            text.text = short_text;

            button.onClick.AddListener(delegate
            {
                show_full = !show_full;
                text.text = show_full ? full_text : short_text;
                update_size = true;
            });
        }

        public void CopyToClipboard()
        {
            copy_button_text.text = "Coppied!";
            GUIUtility.systemCopyBuffer = full_text;
            coppy_button_timer = 3f;
        }


        void Update()
        {
            if (coppy_button_timer > 0)
            {
                coppy_button_timer -= Time.unscaledDeltaTime;
                if (coppy_button_timer <= 0)
                {
                    copy_button_text.text = "Copy";
                }
            }

            if (update_size)
            {
                update_size = false;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, text.preferredHeight + 8f);
            }
        }
    }
}
