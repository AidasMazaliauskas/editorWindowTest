#if UNITY_IOS && !UNITY_EDITOR
    using System.Runtime.InteropServices;
#endif

namespace TutoTOONS
{
    public class AdNetworksAttribution
    {
        #if UNITY_IOS && !UNITY_EDITOR
            [DllImport("__Internal")]
            private static extern void tutotoonsRegisterAttribution();

            [DllImport("__Internal")]
            private static extern void tutotoonsUpdateConversionValue(int _conversion_value);
        #endif

        public static void Register()
        {
            #if UNITY_IOS && !UNITY_EDITOR
                tutotoonsRegisterAttribution();
            #endif
        }

        public static void UpdateConversionValue(int _conversion_value)
        {
            #if UNITY_IOS && !UNITY_EDITOR
                tutotoonsUpdateConversionValue(_conversion_value);
            #endif
        }
    }
}