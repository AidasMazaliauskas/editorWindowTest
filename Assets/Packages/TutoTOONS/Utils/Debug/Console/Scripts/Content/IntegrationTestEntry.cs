using UnityEngine;
using UnityEngine.UI;

namespace TutoTOONS.Utils.Debug.Console
{
    class IntegrationTestEntry : MonoBehaviour
    {
        public UnityEngine.UI.Button button;
        public Text text;
        public RectTransform rt;

        public string full_text;
        private string short_text;
        [SerializeField] private bool show_full;
        private bool update_size = true;

        public void Init()
        {
            update_size = true;
            full_text = text.text;
            short_text = full_text;

            var split = text.text.Split('\n');
            if (split.Length > 0)
            {
                short_text = split[0];
            }

            text.text = show_full ? full_text : short_text;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(delegate
            {
                show_full = !show_full;
                text.text = show_full ? full_text : short_text;
                update_size = true;
            });
        }

        void Update()
        {
            if (update_size)
            {
                update_size = false;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, text.preferredHeight + 8f);
            }
        }

    }
}
