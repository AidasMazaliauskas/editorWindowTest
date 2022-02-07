using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoogleDRMData
{
    public const string STATUS_ALLOW = "allow";
    public const string STATUS_DONT_ALLOW = "dontAllow";
    public const string STATUS_APPLICATION_ERROR = "applicationError";

    public const int APPLICATION_ERROR_INVALID_PACKAGE_NAME = 1;
    public const int APPLICATION_ERROR_NON_MATCHING_UID = 2;
    public const int APPLICATION_ERROR_NOT_MARKET_MANAGED = 3;
    public const int APPLICATION_ERROR_CHECK_IN_PROGRESS = 4;
    public const int APPLICATION_ERROR_INVALID_PUBLIC_KEY = 5;
    public const int APPLICATION_ERROR_MISSING_PERMISSION = 6;
    public const int APPLICATION_ERROR_MISSING_LICENSING_LIBRARY = 7;

    public string status;
    public string signed_data;
    public string signature;
    public string policy_reason;


    public string GetDataString()
    {
        return $"Status: {status} Signed Data {signed_data} Signature: {signature} Policy Reason: {policy_reason}";
    }

}
