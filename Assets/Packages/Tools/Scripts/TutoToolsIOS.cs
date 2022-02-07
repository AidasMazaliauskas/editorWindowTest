using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TutoTOONS
{
    public class TutoToolsIOS : ITutoToolsNativeBridge
    {
    #if UNITY_IOS
        [DllImport("__Internal")] 
        private static extern void toolsUnityInitNativeBridge();
        [DllImport("__Internal")] 
        private static extern void toolsUnityInitTools(string _obj_name);
        [DllImport("__Internal")] 
        private static extern void toolsUnityInitSharedData(string _suite_name);
        [DllImport("__Internal")]
        private static extern void toolsUnitySetDebugMode(bool _enabled);
        [DllImport("__Internal")] 
        private static extern bool toolsUnityContainsDataInPackage(string _data);
        [DllImport("__Internal")] 
        private static extern string toolsUnityGetDeviceId();
        [DllImport("__Internal")] 
        private static extern string toolsUnityGetValue(string _key);
        [DllImport("__Internal")] 
        private static extern void toolsUnitySetValue(string _key, string _value);
        [DllImport("__Internal")] 
        private static extern void toolsUnityRemoveValue(string _key);
    #endif

        public void Init(string _event_object_name)
        {
    #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                toolsUnityInitNativeBridge();
                toolsUnityInitTools(_event_object_name);

                if (!string.IsNullOrEmpty(TutoTools.shared_data_name))
                {
                    toolsUnityInitSharedData(TutoTools.shared_data_name);
                }
                else
                {
                    Debug.LogError($"Failed initializing Shared Data. Provided argument '{nameof(TutoTools.shared_data_name)}' must be not null nor empty");
                }
            }
    #endif
        }

        public void SetDebugMode(bool _enabled)
        {
    #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                toolsUnitySetDebugMode(_enabled);
            }
    #endif
        }

        public bool ContainsDataInPackage(string _data)
        {
    #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return toolsUnityContainsDataInPackage(_data);
            }
    #endif
            return false;
        }

        public void RequestDeviceId()
        {
    #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                TutoTools.instance.TryInvokeEvent(TutoTools.EVENT_ON_RECEIVED_DEVICE_APP_SET_ID, toolsUnityGetDeviceId());
            }
    #endif
        }

        public string GetValue(string _key, string _default_value = "")
        {
    #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if(!string.IsNullOrEmpty(_key) && _key.Length > 0)
                {
                    return toolsUnityGetValue(_key);
                }
            }
    #endif
            return _default_value;
        }

        public void RemoveValue(string _key)
        {
    #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (!string.IsNullOrEmpty(_key) && _key.Length > 0)
                {
                    toolsUnityRemoveValue(_key);
                }
            }
    #endif
        }

        public void SetValue(string _key, string _value)
        {
    #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (!string.IsNullOrEmpty(_key) && !string.IsNullOrEmpty(_value))
                {
                    toolsUnitySetValue(_key, _value);
                }
                else
                {
                    Debug.LogError($"Failed setting preferences. Parameters '{nameof(_key)}' and '{nameof(_value)}' must be non-null and non-empty arguments");
                }
            }
    #endif
        }

        public void RequestDRM(string _public_key, string _uid, int _nonce)
        {

        }
    }
}
