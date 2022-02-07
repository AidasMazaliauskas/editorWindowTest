using System;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS.Tools
{
    internal class Utils
    {
        public const string ANDROID_NATIVE_BRIDGE_CLASS = "com.tutotoons.tools.unity.UnityBridge";
        public const string UNITY_ACTIVITY_CLASS = "com.unity3d.player.UnityPlayer";

        public static TutoToolsEventData ParseEventJson(string _key_value_pair)
        {
            return JsonUtility.FromJson<TutoToolsEventData>(_key_value_pair);
        }

        [Serializable]
        public class TutoToolsEventData
        {
            public string key;
            public string value;
        }
    }
}