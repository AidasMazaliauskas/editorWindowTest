/* This script shows panel ad in scene.
 * Can be configured to work automatically - show panel when this object is created and hide when it's desctroyed,
 * or can be controlled manually by calling Show() and Hide().
 * If ad is requestedto be shown but is not preloaded yet, this controller will show it later when it's ready.
 */

using UnityEngine;
using TutoTOONS;

public class PanelAdController : MonoBehaviour
{
    public bool animationOnClose;   //If true, panel ad disappears with animation.
    public bool autoShow;   //Automatically show panel ad when PanelAdController is created.
    public bool autoClose;  //Automatically close panel ad when PanelAdController is destroyed (in other words - when opening other scene).
    static int STATE_DISABLED = 0;    //Panel ad is not visible
    static int STATE_WAITING = 1;    //Panel ad needs to be shown, but not yet loaded
    static int STATE_SHOWING = 2;    //Panel ad is visible
    int state;
    float checkTimer = 5f;

    public PanelAdController()
    {
        animationOnClose = false;
        autoShow = true;
        autoClose = true;
        state = STATE_DISABLED;
    }

    private void Start()
    {
        if (autoShow && state == STATE_DISABLED)
        {
            state = STATE_WAITING;
            Show();
        }
    }

    public void Show()
    {
#if GEN_SUBSCRIPTION
        if (!TutoTOONS.Subscription.SubscriptionManager.SubscriptionDataConfig.isLoaded) return;
        if (TutoTOONS.Subscription.SubscriptionManager.SubscriptionDataConfig.subscriptionEnabledOnPlatform ||
            !TutoTOONS.Subscription.SubscriptionManager.SubscriptionDataConfig.showPromoPanel)
        {
            return;
        }
#endif
        //Debug.Log("PanelAdController.Show");
        if (state != STATE_SHOWING)
        {
            if (AdServices.CanShowAd(AdLocation.KEYWORD_START_PANEL)
#if IAP_ENABLED
                && IAPController.initComplete && AdServices.state == AdServices.STATE_ENABLED
#endif
                )
            {
                //Debug.Log("Showing panel");
                AdServices.ShowAd(AdLocation.KEYWORD_START_PANEL);
                state = STATE_SHOWING;
            }
            else
            {
                //Debug.Log("Panel can't be shown yet");
                state = STATE_WAITING;
            }
        }
    }

    public void Hide()
    {
        //Debug.Log("PanelAdController.Hide");
        AdServices.HidePanelAd(animationOnClose);
        state = STATE_DISABLED;
    }

    void Update()
    {
#if IAP_ENABLED
        checkTimer -= Time.unscaledDeltaTime;
        if (checkTimer <= 0f) {
            checkTimer = 5f;
#if GEN_BUILDER_PACKAGE
            if (Builder.GameManager.CheckLock(Builder.GameManager.LockType.AdLock))
#else
            if (IAPController.ProductIsPurchased(ProductTypeIAP.NoAds))
#endif
            {
                Hide();
                enabled = false;
            }
        }
#endif

#if GEN_SUBSCRIPTION
        if (state == STATE_SHOWING)
        {
            checkTimer -= Time.unscaledDeltaTime;
            if (checkTimer <= 0f)
            {
                checkTimer = 5f;
                if (TutoTOONS.Subscription.SubscriptionManager.SubscriptionDataConfig.subscriptionEnabledOnPlatform ||
                !TutoTOONS.Subscription.SubscriptionManager.SubscriptionDataConfig.showPromoPanel)
                {
                    animationOnClose = false;
                    Hide();
                }
            }
        }
#endif

        if (state == STATE_WAITING)
        {
            Show();
        }
    }

    void OnDestroy()
    {
        if (autoClose)
        {
            Hide();
        }
    }
}