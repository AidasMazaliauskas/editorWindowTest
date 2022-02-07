using System;

namespace TutoTOONS
{
#if UNITY_IOS
    public class SkadNetworksController
    {
        private const string KEY_IS_MEASUREMENT_PERIOD_OVER = "sk_ad_network_is_measurement_period_over";

        public const int TYPE_DISABLED = 0;    // Disable all SKAdNetworks tracking
        public const int TYPE_MANUAL = 1;      // Conversions updated manually
        public const int TYPE_SINGULAR = 2;    // Uses Singular automated tracking
        public const int TYPE_IRONSOURCE = 3;  // Conversion value updated after each ad from ironSource

        private static int type;
        private static int measurement_period;  // in days
        private static readonly DateTime date_offset = new DateTime(1970, 1, 1);

        public static void Init()
        {
            type = AppConfig.settings.sk_ad_network_controller_type;
            measurement_period = AppConfig.settings.sk_ad_network_controller_measurement_period;

#if GEN_SINGULAR
            if(type != TYPE_SINGULAR)
            {
                SingularSDK.manualSKANConversionManagement = true;

                if (SavedData.first_run)
                {
                    AdNetworksAttribution.Register();
                }
            }
#endif
        }

        public static int GetTypeValue()
        {
            return type;
        }

        public static void UpdateConversionValue(int _conversion_value)
        {
            if (IsMeasurementPeriodOver() || type == TYPE_DISABLED || type == TYPE_SINGULAR)
            {
                return;
            }
            
            AdNetworksAttribution.UpdateConversionValue(_conversion_value);
        }

        private static bool IsMeasurementPeriodOver()
        {
            double _current_timestamp = (DateTime.Now.ToUniversalTime() - date_offset).TotalSeconds;
            double _days_since_install = (int)((_current_timestamp - SavedData.first_launch_timestamp) / TimeSpan.FromDays(1).TotalSeconds);
            bool _is_measurement_over_bool = _days_since_install > measurement_period;

            return _is_measurement_over_bool;
        }
    }
#endif
}