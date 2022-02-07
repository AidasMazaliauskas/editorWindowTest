#if GEN_HAS_UNITY_IAP
using UnityEngine.Purchasing;
#endif

using System.Collections.Generic;

namespace TutoTOONS
{
    public enum ProductTypeIAP
    {
        NoAds,
        UnlockAll,
        Subscription
    }

    public class PurchaseHelper
    {
        #if GEN_HAS_UNITY_IAP

        public static void PurchasedIAP(Product _product, Dictionary<string, object> _attributes = null)
        {
        #if GEN_SINGULAR
        SingularWrapper.PurchasedIAP(_product, false, _attributes);
        #endif
        }

        public static void RestoredIAP(Product _product, Dictionary<string, object> _attributes = null)
        {
        #if GEN_SINGULAR
        SingularWrapper.PurchasedIAP(_product, true, _attributes);
        #endif
        }

        #endif
    }
}
