using UnityEngine;

namespace TutoTOONS
{
    public class FPSTracker
    {
        double total_time;
        double last_minute_time;
        long total_frames;
        long last_minute_frames;
        long minutes_passed;

        public void Init()
        {
            total_time = 0.0;
            last_minute_time = 0.0;
            total_frames = 0;
            last_minute_frames = 0;
            minutes_passed = 0;
        }

        public void Update()
        {
            total_time += Time.unscaledDeltaTime;
            last_minute_time += Time.unscaledDeltaTime;
            ++total_frames;
            ++last_minute_frames;
            if(total_time >= (minutes_passed + 1) * 60.0)
            {
                ++minutes_passed;
                int total_fps = (int)System.Math.Round(total_frames / total_time);
                int minute_fps = (int)System.Math.Round(last_minute_frames / last_minute_time);
                last_minute_time = 0.0;
                last_minute_frames = 0;
                //Track game FPS after 1 and 5 minutes of gameplay
                if (minutes_passed == 1)
                {
                    GoogleAnalytics.TrackEvent("FPS", "After 1 min", null, total_fps);
                }
                else if(minutes_passed == 5)
                {
                    GoogleAnalytics.TrackEvent("FPS", "After 5 min", null, total_fps);
                }
                else
                {
                    /* Track FPS for every 5 minutes, but only if there were no other GA events tracked during that time.
                    * This is used to have correct session length in GA.
                    */
                    if(GoogleAnalytics.time_from_last_call >= 60.0 * 5)
                    {
                        GoogleAnalytics.TrackEvent("FPS", "Minute " + minutes_passed, null, minute_fps);
                    }
                }
            }
        }
    }
}
