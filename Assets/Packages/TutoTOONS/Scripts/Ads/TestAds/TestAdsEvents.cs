using UnityEngine;

namespace TutoTOONS
{
    public class TestAdsEvents : MonoBehaviour
    {
        public void CloseCompleted()
        {
            TestAdsController.instance.CloseAd(true);
        }

        public void CloseNotCompleted()
        {
            TestAdsController.instance.CloseAd(false);
        }
    }
}
