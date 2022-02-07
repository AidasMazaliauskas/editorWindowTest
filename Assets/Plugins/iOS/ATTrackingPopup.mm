#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>

typedef void (*CallbackT)(const char *_message);

extern "C"
{
    void tutotoonsRequestATTAuth(CallbackT callback)
    {
        #if APP_TRACKING_TRANSPARENCY
            if (@available(iOS 14.5, *))
            {
                [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status)
                {
                    if(ATTrackingManagerAuthorizationStatusAuthorized == status)
                    {
                        callback("ATTrackingManagerAuthorizationStatusAuthorized");
                    }
                    else if(ATTrackingManagerAuthorizationStatusDenied == status)
                    {
                        callback("ATTrackingManagerAuthorizationStatusDenied");
                    }
                    else if(ATTrackingManagerAuthorizationStatusRestricted == status)
                    {
                        callback("ATTrackingManagerAuthorizationStatusRestricted");
                    }
                    else if(ATTrackingManagerAuthorizationStatusNotDetermined == status)
                    {
                        callback("ATTrackingManagerAuthorizationStatusNotDetermined");
                    }
                }];
            } else {
                callback("iOSVersionLowerThan14");
            }
        #else
            callback("ATTrackingManagerNotSupported");
        #endif
    }

    void tutotoonsRequestATTState(CallbackT callback)
    {
        #if APP_TRACKING_TRANSPARENCY
            if (@available(iOS 14.5, *))
            {
                ATTrackingManagerAuthorizationStatus status = [ATTrackingManager trackingAuthorizationStatus];
            
                if(ATTrackingManagerAuthorizationStatusAuthorized == status)
                {
                    callback("ATTrackingManagerAuthorizationStatusAuthorized");
                }
                else if(ATTrackingManagerAuthorizationStatusDenied == status)
                {
                    callback("ATTrackingManagerAuthorizationStatusDenied");
                }
                else if(ATTrackingManagerAuthorizationStatusRestricted == status)
                {
                    callback("ATTrackingManagerAuthorizationStatusRestricted");
                }
                else if(ATTrackingManagerAuthorizationStatusNotDetermined == status)
                {
                    callback("ATTrackingManagerAuthorizationStatusNotDetermined");
                }
            } else {
                callback("iOSVersionLowerThan14");
            }
        #else
            callback("ATTrackingManagerNotSupported");
        #endif
    }
}
