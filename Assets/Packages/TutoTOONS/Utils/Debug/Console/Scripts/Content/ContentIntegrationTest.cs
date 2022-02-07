using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TutoTOONS.Utils.Debug.Console
{
    public class ContentIntegrationTest : MonoBehaviour
    {
        public Transform transform_content_list;
        public GameObject prefab_log_entry;
        public ScrollRect scrollRect;
        public RectTransform rt;
        public Text status;
        private Color[] log_colors = { Color.red, Color.green };

        private List<IntegrationTestEntry> integration_test_entries = new List<IntegrationTestEntry>();


        void OnEnable()
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            scrollRect.horizontalNormalizedPosition = 0f;
        }

        public void UpdateTimer(float _timer)
        {
            if (_timer <= -1)
            {
                status.text = "All integration tests passed";
            }
            else
            {
                status.text = $"Time till next attempt: {_timer.ToString("0.00")}";
            }
        }

        public void UpdateTestResult(List<IntegrationTestResult> _test_result)
        {
            IntegrationTestEntry _test_entry = null;
            foreach (IntegrationTestEntry item in integration_test_entries)
            {
                if (item.text.text.Contains(_test_result[0].text))
                {
                    _test_entry = item;
                    break;
                }
            }
            if (_test_entry == null)
            {
                GameObject _game_object = Instantiate(prefab_log_entry, transform_content_list);
                _test_entry = _game_object.GetComponent<IntegrationTestEntry>();
                integration_test_entries.Add(_test_entry);
            }
            _test_entry.text.text = GetFullText(_test_result);
            _test_entry.Init();

            float thres = 40f / rt.sizeDelta.y;

            if (scrollRect.verticalNormalizedPosition <= thres)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private string GetFullText(List<IntegrationTestResult> _integration_test)
        {
            string _result = "";
            for (int i = 0; i < _integration_test.Count; i++)
            {
                _result += GetTextWithColor(_integration_test[i].text, _integration_test[i].result ? 1 : 0) + "\n";
            }
            return _result;
        }

        private string GetTextWithColor(string _text, int _id)
        {
            string result = $"<color=#{ColorUtility.ToHtmlStringRGBA(log_colors[_id])}>{_text}</color>";
            return result;
        }

    }
}
