using System;
using System.Linq;
using UnityEngine;

using TutoTOONS.Cryptography;
using static TutoTOONS.Tools.Utils;
using TutoTOONS.Utils.Debug;

namespace TutoTOONS {

    public class TutoTools : MonoBehaviour
    {
        public const string EVENT_ON_RECEIVED_DEVICE_APP_SET_ID = "receivedDeviceAppSetId";
        public const string EVENT_ON_RECEIVED_GOOGLE_PLAY_DRM = "receivedGooglePlayDRM";

        private static ITutoToolsNativeBridge tutoTools;
        private static bool isInitialized = false;

        public static TutoTools instance;
        // Set by Generator in compile.py
        public static string shared_data_name = "";

        public static Action<string> onReceivedDeviceAppSetId;
        public static Action<GoogleDRMData> onReceivedGooglePlayDRM;

        void Awake()
        {
            if (instance == null)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
            tutoTools = new TutoToolsAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            tutoTools = new TutoToolsIOS();
#else
                tutoTools = new TutoToolsDummy();
#endif
                instance = this;
                Init();

                DontDestroyOnLoad(gameObject);
            }
            else if (instance.gameObject.GetInstanceID() != gameObject.GetInstanceID())
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SDKDebug.InitDebugService(SDKDebug.ServiceName.TutoTools);

            if (SDKDebug.IsServiceInTestMode(SDKDebug.ServiceName.TutoTools))
            {
                SetDebugMode(true);
            }
        }

        public void Init()
        {
            if (!isInitialized)
            {
                tutoTools.Init(gameObject.name);
                isInitialized = true;
            }
        }

        public void SetDebugMode(bool _enabled)
        {
            tutoTools.SetDebugMode(_enabled);
        }

        public void SetValue(string _key, string _value)
        {
            tutoTools.SetValue(AES.Encrypt(_key), AES.Encrypt(_value));
        }

        public string GetValue(string _key, string _default_value = "")
        {
            try
            {
                return AES.Decrypt(tutoTools.GetValue(AES.Encrypt(_key), _default_value));
            }
            catch
            {
                return _default_value;
            }
        }

        public void RemoveValue(string _key)
        {
            tutoTools.RemoveValue(_key);
        }

        public bool ContainsDataInPackage(string _data)
        {
            return tutoTools.ContainsDataInPackage(_data);
        }

        public void RequestDeviceId()
        {
            tutoTools.RequestDeviceId();
        }

        public void RequestDRM(string _public_key, string _uid, int _nonce)
        {
            tutoTools.RequestDRM(_public_key, _uid, _nonce);
        }

        public void TryInvokeEvent(string _event_key,string _event_value)
        {
            try
            {
                switch (_event_key)
                {
                    case EVENT_ON_RECEIVED_DEVICE_APP_SET_ID:
                        onReceivedDeviceAppSetId?.Invoke(_event_value);
                        break;

                    case EVENT_ON_RECEIVED_GOOGLE_PLAY_DRM:
                        GoogleDRMData _google_drm = JsonUtility.FromJson<GoogleDRMData>(_event_value);
                        onReceivedGooglePlayDRM?.Invoke(_google_drm);
                        break;
                    default:
                        Debug.LogWarning($"Event not registered. Received Key = {_event_key}, Values = {_event_value}.");
                        break;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.LogError($"Failed invoking '{_event_key}' event. Too few event arguments provided.");
            }
        }

        private void OnReceiveNativeEvent(string _key_value_pair)
        {
            if (string.IsNullOrEmpty(_key_value_pair))
            {
                return;
            }

            TutoToolsEventData _event_data = ParseEventJson(_key_value_pair);

            if (_event_data.key == null)
            {
                return;
            }        

            TryInvokeEvent(_event_data.key, _event_data.value);
        }
    }
}