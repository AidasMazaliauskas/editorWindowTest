using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TutoTOONS.Utils.Debug.Console
{
    public class Console : MonoBehaviour
    {
        // orientation fix parameters
        public GridLayoutGroup navigation_menu_layout;
        public List<RectTransform> debug_console_contents;
        public RectTransform parent_canvas;
        public RectTransform navigation_menu;

        public ContentAds content_ads;
        public ContentText content_versions;
        public ContentText content_logs;
        public ContentStats content_stats;
        public ContentSDKDebugging content_sdk_debugging;
        public ContentText content_sdk_debugging_logs;
        public ContentIntegrationTest content_integration_tests;

        public GameObject button_prefab;
        public List<GameObject> custom_tabs_buttons;
        public List<GameObject> custom_tabs;

        public bool logsActive = true;

        bool versionsLogged;
        bool buttons_enabled = false;

        private void Awake()
        {
            UpdateSizes();
            HideAllTabs();
        }

        public void LogVersion(string text)
        {
            content_versions.AddLog(text);
        }

        public void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!logsActive) return;

            string _text = logString + "\n" + stackTrace + SceneManager.GetActiveScene().name + "\n" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\n";
            content_logs.AddLog(_text, (int)type);
        }

        public GameObject AddTab(string _name, GameObject _tab)
        {
            GameObject _tab_go = Instantiate(_tab, navigation_menu.parent);
            _tab_go.name = "Tab-" + _name;
            GameObject _button_go = Instantiate(button_prefab, navigation_menu);
            _button_go.GetComponentInChildren<Text>().text = _name;
            _button_go.name = "Button-" + _name;

            UnityEngine.UI.Button _button_component = _button_go.GetComponent<UnityEngine.UI.Button>();
            _button_component.onClick.AddListener(() => ShowCustomTab(_tab_go));

            custom_tabs.Add(_tab_go);
            custom_tabs_buttons.Add(_button_go);

            _tab_go.SetActive(false);
            _button_go.SetActive(buttons_enabled);
            UpdateSizes();
            return _tab_go;
        }

        public void RemoveTab(string _name)
        {
            bool found = false;
            foreach (GameObject item in custom_tabs)
            {
                if (item.name == "Tab-" + _name)
                {
                    found = true;
                    custom_tabs.Remove(item);
                    Destroy(item);
                    break;
                }
            }
            found = false;
            foreach (GameObject item in custom_tabs_buttons)
            {
                if (item.name == "Button-" + _name)
                {
                    found = true;
                    custom_tabs_buttons.Remove(item);
                    Destroy(item);
                    break;
                }
            }
            if (found)
            {
                UpdateSizes(true);
            }
        }

        public void ShowHideButtons()
        {
            buttons_enabled = !buttons_enabled;
            foreach (GameObject _button in custom_tabs_buttons)
            {
                _button.SetActive(buttons_enabled);
            }
            if (!buttons_enabled)
            {
                HideAllTabs();
            }
        }

        public void HideAllTabs()
        {
            foreach (GameObject _custom_tab in custom_tabs)
            {
                _custom_tab.SetActive(false);
            }
        }

        public void ShowCustomTab(GameObject _cutom_tab)
        {
            HideAllTabs();
            _cutom_tab.SetActive(true);
        }

        private void UpdateSizes(bool _after_destroying = false)
        {
            Vector2 _canvas_size = new Vector2(parent_canvas.rect.width, parent_canvas.rect.height);
            int _children_count = navigation_menu_layout.transform.childCount;
            if (_after_destroying)
            {
                // After destroy gameobject is still active till the end of frame
                _children_count--;
            }
            int _coll_count = _children_count;            
            int _row_count = 1;
            bool _is_portrait = false;

#if !UNITY_EDITOR
            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                _coll_count = 4;
                _row_count = (_children_count + _coll_count - 1) / _coll_count;
                _is_portrait= true;
            }
#endif
            navigation_menu_layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            navigation_menu_layout.constraintCount = _row_count;
            float _one_tab_width = (_canvas_size.x - Mathf.Abs(navigation_menu.offsetMin.x) - Mathf.Abs(navigation_menu.offsetMax.x) - _children_count * 8.6f / _row_count) / _coll_count;

            navigation_menu_layout.cellSize = new Vector2(_one_tab_width, 40f);

            foreach (GameObject _tab in custom_tabs)
            {
                RectTransform _transform = _tab.GetComponent<RectTransform>();
                _transform.offsetMax = new Vector2(_transform.offsetMax.x, -96f - 48.6f * (_row_count - 1));
                _transform.offsetMin = new Vector2(0, 50);

                ScrollRect _rect = _tab.GetComponentInChildren<ScrollRect>();
                if (_rect != null)
                {
                    _rect.horizontal = _is_portrait;
                }
            }
        }
    }
}
