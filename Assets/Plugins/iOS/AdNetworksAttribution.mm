#import <StoreKit/StoreKit.h>

extern "C"
{
    void tutotoonsRegisterAttribution()
    {
        if (@available(iOS 11.3, *)) {
            [SKAdNetwork registerAppForAdNetworkAttribution];
        }
    }

    void tutotoonsUpdateConversionValue(int _conversion_value)
    {
        NSInteger _ns_conversion_value = (NSInteger) _conversion_value;
        if (@available(iOS 14.0, *)) {
            [SKAdNetwork updateConversionValue:_ns_conversion_value];
        }
    }
}
