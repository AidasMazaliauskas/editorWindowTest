/* TutoTOONS Package
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TutoTOONS.Utils.Debug;

namespace TutoTOONS
{
    public class TutoTOONS : MonoBehaviour
    {
        private const string AD_OVERLAY_NAME = "AdOverlay";
        /*
            Object with this name will be found in scene and activated while ad is showing
        */

        public static TutoTOONS instance;
        private static Transform adOverlay;
        private FPSTracker fps_tracker;

        public static void SetOverlay(bool _active)
        {
            if (adOverlay != null)
            {
                if (adOverlay.gameObject.activeSelf == _active)
                {
                    return;
                }

                adOverlay.gameObject.SetActive(_active);
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                adOverlay = transform.Find(AD_OVERLAY_NAME);
                SetOverlay(false);
                instance = this;
                DontDestroyOnLoad(gameObject);
                instance.Init();
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        void Init()
        {
            Debug.Log("Bundle ID: " + SystemUtils.GetBundleID() + ", platform: " + Application.platform + ", package version: " + AppConfig.PACKAGE_VERSION);
            SavedData.Init();
            AppConfig.Init();
            ABTesting.Init();
            AdServices.Init();
            GoogleAnalytics.Init();
#if GEN_PLATFORM_UDP
            UnityEngine.UDP.StoreService.Initialize(new InitListenerUDP());
#endif



            fps_tracker = new FPSTracker();
            fps_tracker.Init();

            GoogleAnalytics.TrackPageview("Loader");

            SDKDebug.InitDebugService(SDKDebug.ServiceName.StatsTracker);

            #if !GEN_SINGULAR
            if (SavedData.first_run)
            {
                AdNetworksAttribution.Register();
            }
            #endif
        }

        void Update()
        {
            if (this != instance)
            {
                return;
            }
			SavedData.Update();
            AppConfig.Update();
			ABTesting.Update();
            AdServices.Update();
            GoogleAnalytics.Update();

            fps_tracker.Update();
            SetOverlay(AdServices.state == AdServices.STATE_SHOWING_AD);
        }

        private void OnApplicationQuit()
        {
            SavedData.Save();
        }

        void OnApplicationPause(bool _paused)
        {
            AdServices.OnApllicationPause(_paused);          
        }
    }
}
