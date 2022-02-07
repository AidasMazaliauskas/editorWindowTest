using System;
using UnityEngine;
using UnityEngine.UI;

namespace TutoTOONS
{
    public class TestAdsController : MonoBehaviour
    {
        public static TestAdsController instance;
        public GameObject test_ad_prefab;

        private TestAdsReferences testAdsReferences;
        private Action<bool> close_callback;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void SetCloseCallback(Action<bool> _close_callback)
        {
            close_callback = _close_callback;
        }

        public void ShowAd(string _ad_type, string _ad_location)
        {
            testAdsReferences = Instantiate(test_ad_prefab).GetComponent<TestAdsReferences>();
            testAdsReferences.textAdLocationName.text = _ad_location;
            testAdsReferences.textAdTypeName.text = _ad_type;
        }

        public void CloseAd(bool _completed)
        {
            Destroy(testAdsReferences.gameObject);
            if(close_callback != null)
            {
                close_callback(_completed);
            }
        }
    }
}