using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if GEN_FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif

namespace TutoTOONS
{
    public class SavedData
    {
        public static string player_id;
        public static bool first_run;
        public static int session_num;
        public static double total_play_time;
        public static double first_launch_timestamp;
        public static int first_launch_version_code;
        public static bool initialized;
        private static double autosave_timeout = 1.0;
        private static bool autosave_enabled = true;
        private static int error_counter;

        private static IDictionary<string, string> pairs;
        public const string SAVED_DATA_FILE = "saved_data.json";
        private const string SAVED_DATA_FILE_TEMP = "saved_data_temp.json";
        public const string SAVED_DATA_FILE_BACKUP = "saved_data_backup.json";
        private static bool has_modifications;
        private static double time_since_first_modification;
        private const int ERROR_LIMIT = 5;
        
        public static void Init()
        {
            pairs = new Dictionary<string, string>();
            player_id = null;
            has_modifications = false;
            session_num = 1;
            total_play_time = 0.0;
            first_launch_timestamp = 0.0;
            first_launch_version_code = 0;
            error_counter = 0;

            //Load saved data
            string path = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE);

#if UNITY_ANDROID
            string path_from_PlayerPrefs = PlayerPrefs.GetString("android_persistentDataPath", Application.persistentDataPath);
            if (path_from_PlayerPrefs != Application.persistentDataPath)
            {
#if GEN_FIREBASE_ANALYTICS
                FirebaseAnalytics.LogEvent("saved_data_recovered", "error_cause", "data_path_changed");
#endif
                path = Path.Combine(path_from_PlayerPrefs, SAVED_DATA_FILE);
                PlayerPrefs.SetString("android_persistentDataPath", Application.persistentDataPath);
            }
#endif

            //Debug.Log("SavedData path: " + path);
            if (File.Exists(path))
            {
                SavedDataParserPairs saved_data = GetSavedData();
                
                for (int i = 0; i < saved_data.pairs.Count; i++)
                {
                    //Debug.Log("Adding pair: " + saved_data.pairs[i].k + ", " + saved_data.pairs[i].v);
                    pairs.Add(saved_data.pairs[i].k, saved_data.pairs[i].v);                   
                }
            }

            if (pairs.Count > 0 || PlayerPrefs.GetString("PlayerPref_has_backup", "0") != "0")
            {
                first_run = false;
                //Read parameters
                player_id = GetString("player_id");
                session_num = GetInt("session_num") + 1;
                total_play_time = GetInt("total_play_time");
                first_launch_timestamp = GetDouble("first_launch_timestamp");
                first_launch_version_code = GetInt("first_launch_version_code");

                //Catching SavedData error recovery
                if (pairs.Count == 0)
                {
#if GEN_FIREBASE_ANALYTICS
                    if (File.Exists(path))
                    {
                        FirebaseAnalytics.LogEvent("saved_data_recovered", "error_cause", "file_parse_failed");
                    }
                    else
                    {
                        FirebaseAnalytics.LogEvent("saved_data_recovered", "error_cause", "file_does_not_exist");
                    }
#endif
                }
                //Backing up to PlayerPrefs if they didn't exist beforehand
                else if (PlayerPrefs.GetString("PlayerPref_has_backup", "0") == "0")
                {
                    foreach (KeyValuePair<string, string> pair in pairs)
                    {
                        PlayerPrefs.SetString(pair.Key, pair.Value);
                    }
                }
            }
            else
            {
                first_run = true;
                first_launch_timestamp = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
                first_launch_version_code = SystemUtils.ConvertVersionStringToCode(Application.version);

                //Generate random player ID
                Debug.Log("SavedData doesn't exist, creating new.");
                System.Random rand_gen = new System.Random();
                player_id = System.DateTime.UtcNow.Millisecond.ToString("d3");

                for (int i = 0; i < 13; i++)
                {
                    //Generate random char (digit or uppercase letter)
                    int rand_num = rand_gen.Next(48, 84);
                    if (rand_num > 57)
                    {
                        rand_num += 7;
                    }
                    player_id += (char)rand_num;
                }

                PlayerPrefs.SetString("android_persistentDataPath", Application.persistentDataPath);
            }

            Save();

            PlayerPrefs.SetString("PlayerPref_has_backup", "1");

            initialized = true;
        }

        public static void DeleteKey(string key)
        {
            if (pairs == null)
            {
                return;
            }
            if (pairs.ContainsKey(key))
            {
                pairs.Remove(key);
            }
            PlayerPrefs.DeleteKey(key);

            SetModified();
        }

        public static bool HasKey(string key)
        {
            if (!initialized)
            {
                Init();
            }

            return pairs.ContainsKey(key) || PlayerPrefs.HasKey(key);
        }

        public static void SetInt(string key, int val)
        {
            if (pairs != null)
            {
                pairs[key] = val.ToString();
                PlayerPrefs.SetString(key, val.ToString());
            }

            SetModified();
        }

        public static int GetInt(string key, int default_val = 0)
        {
            if (pairs.ContainsKey(key))
            {
                return Int32.Parse(pairs[key]);
            }
            else if (PlayerPrefs.HasKey(key))
            {
                return Int32.Parse(PlayerPrefs.GetString(key, default_val.ToString()));
            }
            return default_val;
        }

        public static void Clear()
        {
            string _path = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE);
            if (File.Exists(_path))
            {
                Debug.Log("Data cleared from " + SAVED_DATA_FILE);
                File.Delete(_path);
            }
            string _path_temp = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE_TEMP);
            if (File.Exists(_path_temp))
            {
                Debug.Log("Data cleared from " + SAVED_DATA_FILE_TEMP);
                File.Delete(_path_temp);
            }
            string _path_backup = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE_BACKUP);
            if (File.Exists(_path_backup))
            {
                Debug.Log("Data cleared from " + SAVED_DATA_FILE_BACKUP);
                File.Delete(_path_backup);
            }
#if !UNITY_EDITOR
            pairs.Clear();
#endif
        }

        public static void SetDouble(string key, double val)
        {
            if (pairs != null)
            {
                pairs[key] = val.ToString();
                PlayerPrefs.SetString(key, val.ToString());
            }

            SetModified();
        }

        public static double GetDouble(string key, double default_val = 0.0)
        {
            if (pairs.ContainsKey(key))
            {
                return MathUtils.ParseDouble(pairs[key], default_val);
            }
            else if (PlayerPrefs.HasKey(key))
            {
                return MathUtils.ParseDouble(PlayerPrefs.GetString(key, default_val.ToString()), default_val);
            }
            else
            {
                return default_val;
            }
        }

        public static void SetString(string key, string val)
        {
            if (pairs == null)
            {
                return;
            }

            pairs[key] = val;
            PlayerPrefs.SetString(key, val);

            SetModified();
        }

        public static string GetString(string key, string default_val = null)
        {
            if (pairs.ContainsKey(key))
            {
                return pairs[key];
            }
            else if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key, default_val);
            }
            else
            {
                return default_val;
            }
        }

        public static void SetJSON(string key, string val)
        {
            val = val.Replace("\"", "\\\"");
            SetString(key, val);
        }

        public static string GetJSON(string key, string default_val = null)
        {
            string val = GetString(key, default_val);
            if (val == null) return null;
            val = val.Replace("\\\"", "\"");
            return val;
        }

        private static void SetModified()
        {
            if (!has_modifications)
            {
                has_modifications = true;
                time_since_first_modification = 0.0;
            }
        }

        public static bool Save(IDictionary<string, string> _pairs = null)
        {
            if (!SetSaveParameters(_pairs))
            {
                Debug.LogError("Failed saving data.");
                return false;
            }

            StringBuilder _file_data = GenerateJSON(_pairs ?? pairs);        
            SaveToFile(_file_data);
            PlayerPrefs.Save();

            has_modifications = false;
            return true;
        }

        private static bool SetSaveParameters(IDictionary<string, string> _pairs = null)
        {
            if(_pairs != null)
            {
                try
                {
                    player_id = _pairs["player_id"];
                    session_num = int.Parse(_pairs["session_num"]);
                    total_play_time = int.Parse(_pairs["total_play_time"]);
                    first_launch_timestamp = double.Parse(_pairs["first_launch_timestamp"]);
                    first_launch_version_code = int.Parse(_pairs["first_launch_version_code"]);
                }
                catch
                {
                    Debug.LogError("Failed parsing save parameters");
                    return false;
                }
            }

            SetString("player_id", player_id);
            SetInt("session_num", session_num);
            SetInt("total_play_time", (int)System.Math.Round(total_play_time));
            SetDouble("first_launch_timestamp", first_launch_timestamp);
            SetInt("first_launch_version_code", first_launch_version_code);

            return true;
        }

        private static StringBuilder GenerateJSON(IDictionary<string, string> _pairs)
        {
            StringBuilder _file_data = new StringBuilder();
            _file_data.Append("{\n\t\"pairs\":[");
            bool first = true;

            foreach (KeyValuePair<string, string> pair in _pairs)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    _file_data.Append(",");
                }
                String _val = pair.Value;
                if (_val != null)
                {
                    if (_val.Length > 0)
                    {
                        if (_val[0] == '{')
                        {
                            _val = Regex.Replace(_val, "(?<!\\\\)\"", "\\\"");
                        }
                    }
                }
                _file_data.Append("\n\t\t{\n\t\t\t\"k\":\"");
                _file_data.Append(pair.Key);
                _file_data.Append("\",\n\t\t\t\"v\":\"");
                _file_data.Append(_val);
                _file_data.Append("\"\n\t\t}");
            }
            _file_data.Append("\n\t]\n}");

            return _file_data;
        }

        public static void Export(IDictionary<string, string> _pairs_to_save, string _destination)
        {
            StringBuilder _file_data = GenerateJSON(_pairs_to_save);
            SaveToFile(_file_data, _destination);          
        }

        private static void AddPadding(StringBuilder _file_data)
        {
            int min_file_size = 100000; // 100 KB
            StringBuilder _padding = new StringBuilder();

            if (_file_data.Length < min_file_size)
            {
                _file_data.Append("\n");
                _file_data.Append(' ', min_file_size - _file_data.Length);
                _file_data.Append(_padding.ToString());
            }
        }

        private static bool IsJsonFileValid(string _path)
        {
            if (!File.Exists(_path))
            {
                string _error_message = "SavedData::IsJsonFileValid: saved data doesn't exists";
                SendError(_error_message);
                Debug.Log(_error_message);
                return false;
            }
            try
            {
                string _json_data = File.ReadAllText(_path);
                SavedDataParserPairs _parsed_pairs = JsonUtility.FromJson<SavedDataParserPairs>(_json_data);
            }
            catch (Exception e)
            {
                string _error_message = "SavedData::IsJsonFileValid: " + e.Message;
                SendError(_error_message);
                Debug.Log(_error_message);
                return false;
            }
            return true;
        }

        private static void SaveToFile(StringBuilder _data)
        {
            string _path = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE);
            string _path_temp = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE_TEMP);
            string _path_backup = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE_BACKUP);

            AddPadding(_data);
            File.WriteAllText(_path_temp, _data.ToString());

            if (IsJsonFileValid(_path_temp))
            {
                if (File.Exists(_path))
                {
                    File.Replace(_path_temp, _path, _path_backup);
                }
                else
                {
                    File.Copy(_path_temp, _path);
                }
            }
        }

        private static void SaveToFile(StringBuilder _data, string _path_to_save)
        {
            AddPadding(_data);

            string _temp_file = Guid.NewGuid().ToString() + ".json";
            File.WriteAllText(_temp_file, _data.ToString());

            if (IsJsonFileValid(_temp_file))
            {
                File.WriteAllText(_path_to_save, File.ReadAllText(_temp_file));
            }

            File.Delete(_temp_file);
        }

        public static SavedDataParserPairs ReadSavedData(string _path)
        {
            if (File.Exists(_path))
            {
                try
                {
                    string _json_data = File.ReadAllText(_path);
                    SavedDataParserPairs _parsed_pairs = JsonUtility.FromJson<SavedDataParserPairs>(_json_data);
                    return _parsed_pairs;
                }
                catch (Exception e)
                {
                    string _error_message = "SavedData::GetSavedData: Failed To read " + _path + " " + e.Message;
                    SendError(_error_message);
                    Debug.Log(_error_message);
                }
            }
            return null;
        }

        private static SavedDataParserPairs GetSavedData()
        {
            string _path = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE);
            string _path_temp = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE_TEMP);
            string _path_backup = Path.Combine(Application.persistentDataPath, SAVED_DATA_FILE_BACKUP);

            SavedDataParserPairs _pairs = new SavedDataParserPairs();

            _pairs = ReadSavedData(_path);
            if (_pairs != null)
            {
                return _pairs;
            }

            _pairs = ReadSavedData(_path_temp);
            if (_pairs != null)
            {
                return _pairs;
            }

            _pairs = ReadSavedData(_path_backup);
            if (_pairs != null)
            {
                return _pairs;
            }

            return new SavedDataParserPairs();
        }

        private static void SendError(string _message)
        {
            if (error_counter < ERROR_LIMIT)
            {
                error_counter++;
                string ERROR_LOG_URI = "https://api.tutotoons.com/v3/insert/temp_event?title=test&bundle_id=" + SystemUtils.GetBundleID();

                string param1 = "saved_data_error_v2";
                string param2 = "error_message";
                UnityWebRequest webRequest = new UnityWebRequest();
                string finalURL = ERROR_LOG_URI + "&param1=" + param1 + "&param2=" + param2 + "&event_data=" + _message;
                webRequest.url = finalURL;
                webRequest.SendWebRequest();
            }
        }

        public static void Update()
        {
            total_play_time += Time.unscaledDeltaTime;
            if (has_modifications)
            {
                time_since_first_modification += Time.unscaledDeltaTime;
                if (autosave_enabled && time_since_first_modification >= autosave_timeout)
                {
                    Save();
                }
            }
        }

        public static void SetAutosaveTimeout(double _seconds)
        {
            autosave_timeout = _seconds;
        }

        public static void SetAutosaveState(bool _enabled)
        {
            autosave_enabled = _enabled;
        }
    }

    [Serializable]
    public class SavedDataParserPairs
    {
        public List<SavedDataParserPair> pairs;
    }

    [Serializable]
    public class SavedDataParserPair
    {
        public string k;
        public string v;
    }
}