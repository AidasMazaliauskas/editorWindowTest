using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using static TutoTOONS.Utils.Debug.Console.DebugConsoleCommand;

namespace TutoTOONS.Utils.Debug.Console
{
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole instance;
        private const string KEY_SAVED_STATE = "debug_console_enabled";
        private const int CLICK_THRESHOLD = 7;
        private const int INPUT_ACTIVATION_LIMIT = 15;
        private const string KEY = "1af180faed9a8717cf05d3fb75d734e11a563b70fd694001bd02513f76a4ff16117488bdae49a527a3173b803693b82d74cb96e7d5b91f8a5434a31d1c4b4cd4";

        public Input input;
        public Console console;
        public InputField input_field;
        public GameObject consoleContainer;
        public Text txtGameInfo;


        public GameObject content_ads_prefab;
        public GameObject content_versions_prefab;
        public GameObject content_logs_prefab;
        public GameObject content_stats_prefab;
        public GameObject content_sdk_debugging_prefab;
        public GameObject content_sdk_debugging_logs_prefab;
        public GameObject content_integration_tests_prefab;


        Dictionary<string, DebugConsoleCommand> command_list = new Dictionary<string, DebugConsoleCommand>();
        Dictionary<string, DebugConsoleCommand> locked_command_list = new Dictionary<string, DebugConsoleCommand>();


        public bool console_active { get; private set; }
        private bool input_active;
        private double input_timeout;

        void OnEnable()
        {
            Application.logMessageReceived += console.HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= console.HandleLog;
        }
        public void UpdateLastAdInfo(string _ad_network, string _ad_type, bool _reward_given)
        {
            var txt = "Last completed ad info:" + "\n";
            txt += "Ad network: " + _ad_network + "\n";
            txt += "Ad type: " + _ad_type + "\n";
            txt += "Reward given: " + _reward_given + "\n";
            console.content_ads.UpdateLastAdInfo(txt);
        }

        public static void AddCommand(string _name, string _description, CommandCallback _callback, bool _enable_on_locked_console = false)
        {
            if (_enable_on_locked_console)
            {
                if (instance.locked_command_list.ContainsKey(_name))
                {
                    instance.locked_command_list[_name] = new DebugConsoleCommand(_name, _description, _callback);
                }
                else
                {
                    instance.locked_command_list.Add(_name, new DebugConsoleCommand(_name, _description, _callback));
                }
            }

            if (instance.command_list.ContainsKey(_name))
            {
                instance.command_list[_name] = new DebugConsoleCommand(_name, _description, _callback);
            }
            else
            {
                instance.command_list.Add(_name, new DebugConsoleCommand(_name, _description, _callback));
            }
        }

        public static GameObject AddTab(string _name, GameObject _tab)
        {
            return instance.console.AddTab(_name, _tab);
        }

        public static void RemoveTab(string _name)
        {
            instance.console.RemoveTab(_name);
        }

        public static void AddSDKVersion(List<PackageVersion> _package_versions)
        {
            if(_package_versions == null)
            {
                return;
            }

            StringBuilder _log_entry = new StringBuilder();
            
            foreach(PackageVersion _version in _package_versions)
            {
                _log_entry.Append($"{_version}\n");
            }

            instance.console.LogVersion(_log_entry.ToString());
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                InitDefaultTabs();
                console.content_sdk_debugging.Init();
                PackageVersions.Init();
            }
            else
            {
                Destroy(this);
            }
        }

        void Start()
        {
            InitializeCommands();
            input_field.onEndEdit.AddListener(onInputFieldChange);

            console_active = false;
            input_active = false;
            input_timeout = 0.0;
            console.gameObject.SetActive(false);
            input.SetActive(false);         

#if !UNITY_EDITOR
            if (SavedData.GetInt(KEY_SAVED_STATE) > 0)
            {
                Show();
                ShowUnlockField();
            }
#endif
        }

        private void TryShow()
        {
            if (input_active)
            {
                return;
            }

            if (ActionButton.actions_couted >= CLICK_THRESHOLD)
            {
                ActionButton.Reset();
                ShowUnlockField();
                input_active = true;
                input_timeout = INPUT_ACTIVATION_LIMIT;
            }
        }

        private void Show()
        {
            if (console_active)
            {
                return;
            }
            consoleContainer.SetActive(true);
            console_active = true;
            console.gameObject.SetActive(true);
            SavedData.SetInt(KEY_SAVED_STATE, 1);
        }

        private void Close(string _command = "")
        {
            ActionButton.Reset();
            input_active = false;
            console_active = false;
            consoleContainer.SetActive(false);
            console.gameObject.SetActive(false);
            input.SetActive(false);
            SavedData.SetInt(KEY_SAVED_STATE, 0);
        }

        private void ShowUnlockField()
        {
            if (input_active)
            {
                return;
            }

            input.SetActive(true);
        }

        private void InitDefaultTabs()
        {
            console.content_ads = console.AddTab("Ads", content_ads_prefab).GetComponent<ContentAds>();
            console.content_versions = console.AddTab("Versions", content_versions_prefab).GetComponent<ContentText>();
            console.content_logs = console.AddTab("Logs", content_logs_prefab).GetComponent<ContentText>();
            console.content_stats = console.AddTab("Stats", content_stats_prefab).GetComponent<ContentStats>();
            console.content_sdk_debugging = console.AddTab("SDK Debugging", content_sdk_debugging_prefab).GetComponent<ContentSDKDebugging>();
            console.content_sdk_debugging_logs = console.AddTab("SDK Debugging Logs", content_sdk_debugging_logs_prefab).GetComponent<ContentText>();
            console.content_integration_tests = console.AddTab("Integration tests", content_integration_tests_prefab).GetComponent<ContentIntegrationTest>();
        }

        private void onInputFieldChange(string _entered_text)
        {
            input_field.GetComponent<InputField>().text = "";
            string _command = _entered_text.Split(' ')[0];

            if (!console_active)
            {
                if (computeHash(_entered_text) == KEY)
                {
                    Show();
                }
                else if (locked_command_list.ContainsKey(_command)) 
                {
                    locked_command_list[_command].commandCallback.Invoke(_entered_text);
                }
                else
                {
                    Close();
                }
                return;
            }
            
            if (string.IsNullOrEmpty(_command))
            {
                return;
            }
            if (command_list.ContainsKey(_command))
            {
                command_list[_command].commandCallback.Invoke(_entered_text);
            }
            else
            {
                console.content_logs.AddLog($"Command '{_command}' not found, use 'help' to get all commands", (int)LogType.Log);
            }
        }

        private void InitializeCommands()
        {
            AddCommand("close", "Closes console", Close);
            AddCommand("help", "List of all commands", ListAllCommands);
        }

        private void ListAllCommands(string _entered_text)
        {
            string _text = $"List of all available commands:";
            foreach (string _key in command_list.Keys)
            {
                _text += $"\n'{_key}' - {command_list[_key].description}";
            }
            console.content_logs.AddLog(_text, (int)LogType.Log);
        }

        private string computeHash(string _data)
        {
            using (SHA512 _sha_512 = SHA512.Create())
            {
                StringBuilder _builder = new StringBuilder();
                byte[] _bytes = _sha_512.ComputeHash(Encoding.UTF8.GetBytes(_data));

                for (int i = 0; i < _bytes.Length; i++)
                {
                    _builder.Append(_bytes[i].ToString("x2"));
                }
                return _builder.ToString();
            }
        }

        void UpdateGameInfo()
        {
            string txt = $"Game ID: {Application.identifier}";
            txt += $"\nScene ID: {SceneManager.GetActiveScene().name}";
            txt += $"\nCurrent time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}   ";
            txt += $"FPS: {(int)(1f / Time.unscaledDeltaTime),-3}";
            txtGameInfo.text = txt;
        }

        void Update()
        {
            if (console_active)
            {
                UpdateGameInfo();
            }

            TryShow();

            if (input_active && !console_active)
            {
                input_timeout -= Time.unscaledDeltaTime;

                if (input_timeout <= 0)
                {
                    Close();
                }
            }
        }
    }
}