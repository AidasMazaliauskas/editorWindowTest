using System;
using System.Collections.Generic;
#if GEN_PLATFORM_UDP
using UnityEngine.UDP.Editor;
#endif

// object parsed from UDP console


[Serializable]
public class UDPConfig
{
#if GEN_PLATFORM_UDP
    public AppItem appItem;
    public List<IapItem> iapItems;
    public List<TestAccount> players;
    public UdpClient client;
#endif
}
