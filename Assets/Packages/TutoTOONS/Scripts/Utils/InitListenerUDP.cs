#if GEN_PLATFORM_UDP
using UnityEngine;
using UnityEngine.UDP;

public class InitListenerUDP : IInitListener
{
    public void OnInitialized(UserInfo userInfo)
    {
        Debug.Log("UDP SDK Initialized successfully!");    
    }

    public void OnInitializeFailed(string message)
    {
        Debug.Log("UDP SDK Failed to initialize: " + message);
    }
}
#endif