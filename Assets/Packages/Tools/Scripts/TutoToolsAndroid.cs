using UnityEngine;

using TutoTOONS.Tools;
using static TutoTOONS.Tools.Utils;

namespace TutoTOONS
{
    public class TutoToolsAndroid : ITutoToolsNativeBridge
    {
        private AndroidJavaObject activity;
        private AndroidJavaObject context;
        private AndroidJavaObject nativeBridge;

        private void InitNativeBridge(string _event_object_name)
        {
            if (Application.isEditor)
            {
                return;
            }

            using (AndroidJavaClass activityClass = new AndroidJavaClass(UNITY_ACTIVITY_CLASS))
            {
                activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                context = activity.Call<AndroidJavaObject>("getApplicationContext");
            }

            AndroidJavaClass nativeBridgeClass = new AndroidJavaClass(ANDROID_NATIVE_BRIDGE_CLASS);
            nativeBridge = nativeBridgeClass.CallStatic<AndroidJavaObject>("getInstance", activity);
            nativeBridge.CallStatic("setEventObjectName", _event_object_name);
        }

        public void Init(string _event_object_name)
        {
            if(nativeBridge == null)
            {
                InitNativeBridge(_event_object_name);
            }

            nativeBridge.CallStatic("init", context);
        }

        public void SetDebugMode(bool _enabled)
        {
            if (nativeBridge == null || activity == null)
            {
                return;
            }

            nativeBridge.CallStatic("setDebugMode", _enabled);
        }

        public void SetValue(string _key, string _value)
        {
            if (nativeBridge == null || activity == null)
            {
                return;
            }

            nativeBridge.CallStatic("setValue", _key, _value);
        }

        public string GetValue(string _key, string _default_value = "")
        {
            if (nativeBridge == null || activity == null)
            {
                return _default_value;
            }

            string result = nativeBridge.CallStatic<string>("getValue", _key, _default_value);

            if (string.IsNullOrEmpty(result))
            {
                return _default_value;
            }

            return result;
        }

        public void RemoveValue(string _key)
        {
            if (nativeBridge == null || activity == null)
            {
                return;
            }

            nativeBridge.CallStatic("removeValue", _key);
        }

        public bool ContainsDataInPackage(string _data)
        {
            if (nativeBridge == null || activity == null)
            {
                return false;
            }

            return nativeBridge.CallStatic<bool>("containsDataInPackage", _data);
        }

        public void RequestDeviceId()
        {
            if (nativeBridge != null && activity != null)
            {
                nativeBridge.CallStatic("requestDeviceId");
            }
        }

        public void RequestDRM(string _public_key, string _uid, int _nonce)
        {
            if (nativeBridge != null && activity != null)
            {
                nativeBridge.CallStatic("requestGooglePlayDRM", _public_key, _uid, _nonce);
            }
        }

    }
}
