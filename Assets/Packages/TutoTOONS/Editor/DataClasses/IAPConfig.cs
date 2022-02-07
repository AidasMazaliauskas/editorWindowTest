using System.Collections.Generic;
using System;

// IAP items list parsed from tutotoons api 

[Serializable]
public class IAPConfig
{
    public bool is_consumable
    {
        get
        {
            return consumable == "1";
        }
    }
    public string in_app_purchase_id;
    public string production_app_id;
    public string bundle_id_suffix;
    public string enable_money;
    public string enable_coins;
    public string consumable;
    public string price_tier;
    public string price_coins;
    public string created;
    public string modified;
    public string deleted;
    public List<string> langs;
    public string title;
    public string description;
    public List<DescriptionsExtra> descriptions_extra;
}
