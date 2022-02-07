using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SkadNetworkData
{
    public Skadnetworks skadnetworks;
}

[Serializable]
public class AdNetwork
{
    public string name;
    public List<string> skadnetworks_ids;
}

[Serializable]
public class Mediation
{
    public List<AdNetwork> ad_networks;
}

[Serializable]
public class Skadnetworks
{
    public Mediation ironsource_mediation, admob_mediation, applovin_max_mediation;
}


