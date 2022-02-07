using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    public class AnalyticsHelper
    {
        public static void TrackAdImpressions(string _ad_network_title, string _ad_event_type, AdLocation _ad_location, string _ad_type)
        {
            try
            {
                if (_ad_type == AdData.TYPE_PANEL)
                {
                    return;
                }

                switch (_ad_event_type)
                {
                    case AdService.EVENT_SHOW:
                        //Ad Finished Counter
                        int _total_ads_finished = SavedData.GetInt("total_ads_finished");
                        _total_ads_finished++;
                        SavedData.SetInt("total_ads_finished", _total_ads_finished);
                        
                        TrackAdImpressionEvent(_ad_network_title, _ad_type, _total_ads_finished.ToString());
                        TrackAdDayImpressionEvents(_ad_network_title, _ad_type, _total_ads_finished.ToString());
                        break;

                    case AdService.EVENT_CLICK:

                        break;

                    case AdService.EVENT_FINISHED:

                        break;
                }
            }
            catch (Exception e)
            {
                //Debug.Log("Error while sending firebase event");
            }
        }

        private static void TrackAdImpressionEvent(string _ad_network_title, string _ad_type, string _ad_impression_count)
        {
            #if GEN_SINGULAR
            SingularWrapper.TrackAdImpressionEvent(_ad_network_title, _ad_type, _ad_impression_count);
            #endif

            #if GEN_FIREBASE_ANALYTICS
            FirebaseWrapper.TrackAdImpressionEvent(_ad_network_title, _ad_type, _ad_impression_count);
            #endif

        }

        private static void TrackAdDayImpressionEvents(string _ad_network_title, string _ad_type, string _ad_impression_count)
        {
            int _days_since_intall = -1;

            if (SavedData.first_launch_timestamp > 0)
            {
                double _current_timestamp = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
                _days_since_intall = (int)((_current_timestamp - SavedData.first_launch_timestamp) / 86400);
            }

            if(_days_since_intall < 0)
            {
                return;
            }

            #if GEN_SINGULAR
            SingularWrapper.TrackAdFirst24hImpressions(_days_since_intall, _ad_impression_count);
            #endif
            
            #if GEN_FIREBASE_ANALYTICS
            FirebaseWrapper.TrackAdImpressionDay(_days_since_intall.ToString());
            FirebaseWrapper.TrackAdFirst24hImpressions(_days_since_intall, _ad_impression_count);
            #endif
        }
    }
}
