using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TutoTOONS
{
    public class SceneFPSTracker : MonoBehaviour
    {
        public static SceneFPSTracker instance;

        public bool autoStart = true;
        public string locationName = "default";

        private const float DEFAULT_DELAY = 5f;
        private const float DEFAULT_MEASURE_DURATION = 10f;
        private const float DEFAULT_TEXT_SHOW_DURATION = 10f;
        private const string DEFAULT_CATEGORY = "FPS";

        private int framesElapsed;
        private float measureTime;
        private bool measuringInProgress;
        private Text debugText;

        private void Awake()
        {    
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogWarning("Found another SceneFPSTracker instance, destroying this script...");
                Destroy(this);
            }
        }

        private void Start()
        {
            if (autoStart)
            {
                StartMeasuring();
            }
        }

        private void Update()
        {
            if (!measuringInProgress) 
                return;
            
            if (measureTime >= DEFAULT_MEASURE_DURATION)
            {
                TryTrackEvent();

                measuringInProgress = false;
                framesElapsed = 0;
                measureTime = 0;
            }

            framesElapsed++;
            measureTime += Time.unscaledDeltaTime;
        }

        /// <summary>
        /// Starts measuring FPS if it hasn't measured already
        /// </summary>
        public void StartMeasuring()
        {
            Invoke(nameof(BeginMeasuring), DEFAULT_DELAY);
        }

        /// <summary>
        /// Starts measuring FPS if it hasn't measured already
        /// </summary>
        /// <param name="_customDelay">Custom delay before begin measuring</param>
        public void StartMeasuring(float _customDelay)
        {
            Invoke(nameof(BeginMeasuring), _customDelay);
        }

        /// <summary>
        /// Starts measuring FPS if it hasn't measured already
        /// </summary>
        /// <param name="customDelay">Custom delay before begin measuring</param>
        /// <param name="customLocation">Custom location to track FPS</param>
        public void StartMeasuring(float customDelay, string customLocation)
        {
            locationName = customLocation;
            Invoke(nameof(BeginMeasuring), customDelay);
        }

        /// <summary>
        /// Stops fps measuring
        /// </summary>
        /// <param name="shouldTrack">Should fps event be tracked now</param>
        public void StopMeasuring(bool shouldTrack)
        {
            if (!measuringInProgress)
                return;
            
            if (shouldTrack)
            {
                TryTrackEvent();
            }

            measuringInProgress = false;
            framesElapsed = 0;
            measureTime = 0;
        }

        private void BeginMeasuring()
        {
            if (IsAlreadyTracked)
            {
                Debug.Log($"FPS already tracked in '{GetTrackingLocation}'");
                return;
            }
            
            Debug.Log($"Started measuring FPS for '{GetTrackingLocation}'");
            measuringInProgress = true;
        }

        private void TryTrackEvent()
        {
            int _averageFPS = Mathf.RoundToInt((float)framesElapsed / measureTime);
            bool _debugEnabled = false;
            
#if UNITY_EDITOR
            TrackConsole(_averageFPS);
            return;
#endif
            
            if (Utils.Debug.Console.DebugConsole.instance)
            {
                _debugEnabled = Utils.Debug.Console.DebugConsole.instance.console_active;
            }

            if (_debugEnabled)
            {
                TrackConsole(_averageFPS);
            }
            else
            {
                if (IsAlreadyTracked)
                {
                    Debug.Log($"FPS already tracked in '{GetTrackingLocation}'. This should never happen.");
                    return;
                }

                TrackAnalytics(_averageFPS);
            }
        }

        private void TrackConsole(int averageFPS)
        {
            string _logText = $"'{GetTrackingLocation}' average FPS: {averageFPS}. Measure duration: {DEFAULT_MEASURE_DURATION}s.";

#if UNITY_EDITOR
            Debug.Log(_logText);
            return;
#endif
            
            if (debugText)
            {
                debugText.text = _logText;
                debugText.gameObject.SetActive(true);
            }
            else
            {
                CreateDebugText(_logText);
            }
            Debug.Log(_logText);
            Invoke(nameof(HideDebugText), DEFAULT_TEXT_SHOW_DURATION);
        }

        private void TrackAnalytics(int averageFPS)
        {
            GoogleAnalytics.TrackEvent(DEFAULT_CATEGORY, GetTrackingActionName, averageFPS.ToString(), averageFPS);
            SavedData.SetInt(GetFormattedSaveKey, 1);
            Debug.Log($"FPS tracked successfully for '{GetTrackingLocation}'");
        }

        private void CreateDebugText(string _text)
        {
            GameObject _container = new GameObject();
            _container.name = "FPS measure container";
            _container.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            GameObject _textObj = new GameObject("Text");
            _textObj.transform.parent = _container.transform.transform;
            RectTransform _textTransform = _textObj.AddComponent<RectTransform>();
            _textTransform.anchorMin = new Vector2(1, 1);
            _textTransform.anchorMax = new Vector2(1, 1);
            _textTransform.anchoredPosition = new Vector2(-230, -200);
            debugText = _textObj.AddComponent<Text>();
            debugText.alignment = TextAnchor.MiddleCenter;
            debugText.verticalOverflow = VerticalWrapMode.Overflow;
            debugText.horizontalOverflow = HorizontalWrapMode.Overflow;
            debugText.fontSize = 30;
            debugText.fontStyle = FontStyle.Bold;
            debugText.raycastTarget = false;
            debugText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            debugText.text = _text;
        }

        private void HideDebugText()
        {
            if (debugText)
            {
                debugText.gameObject.SetActive(false);
            }
        }
        
        private string GetTrackingActionName
        {
            get
            {
                return GetTrackingLocation + " " + "(" + GetTrackingPlatform + ")";
            }
        }
        
        private string GetTrackingLocation
        {
            get
            {
                return locationName.ToLower().Trim() == "default" ? SceneManager.GetActiveScene().name : locationName;
            }
        }
        
        private string GetTrackingPlatform
        {
            get
            {
                string _platform = "Editor";
#if GEN_PLATFORM_AMAZON
                _platform = "Amazon";
#elif UNITY_ANDROID
                _platform = "Android";
#elif UNITY_IOS
                _platform = "iOS";
#endif
                return _platform;
            }
        }

        private string GetFormattedSaveKey
        {
            get
            {
                return "tracked " + DEFAULT_CATEGORY + "_" + GetTrackingActionName;;
            }
        }

        private bool IsAlreadyTracked
        {
            get
            {
#if UNITY_EDITOR
                return false;
#endif
                return SavedData.HasKey(GetFormattedSaveKey);
            }
        }
    }
}