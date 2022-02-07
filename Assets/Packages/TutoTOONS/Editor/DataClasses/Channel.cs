using System.Collections.Generic;
using System;


// object parsed from UDP console
[Serializable]
public class Channel
{
    public string projectGuid;
    public List<string> thirdPartySettings;
    public string callbackUrl;
    public string publicRSAKey;
    public string channelSecret;
}
