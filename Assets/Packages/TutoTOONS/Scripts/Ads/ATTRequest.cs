#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using AOT;
#endif

using System;
using UnityEngine;

namespace TutoTOONS
{
    public class ATTRequest
    {
        public const string ATT_MESSAGE = "By allowing tracking, we'll be able to continue developing the game and improving your in-game experience, including better tailored-ads";
        public const string ATT_CALLBACK_AUTHORIZED = "ATTrackingManagerAuthorizationStatusAuthorized";
        public const string ATT_CALLBACK_DENIED = "ATTrackingManagerAuthorizationStatusDenied";
        public const string ATT_CALLBACK_RESTRICTED = "ATTrackingManagerAuthorizationStatusRestricted";
        public const string ATT_CALLBACK_NOT_DETERMINED = "ATTrackingManagerAuthorizationStatusNotDetermined";
        public const string ATT_CALLBACK_IOS_LOWER_THAN_14 = "iOSVersionLowerThan14";
        public const string ATT_CALLBACK_NOT_SUPPORTED = "ATTrackingManagerNotSupported";

        public delegate void ATTRequestResponse(string _status);
        public delegate void ATTStatusResponse(string _status);
        private static ATTRequestResponse att_request_response;
        private static ATTStatusResponse att_status_response;

        #if UNITY_IOS && !UNITY_EDITOR
        delegate void Callback(IntPtr foo);
        [DllImport("__Internal")]
        private static extern void tutotoonsRequestATTAuth([MarshalAs(UnmanagedType.FunctionPtr)] Callback callback);
        [DllImport("__Internal")]
        private static extern void tutotoonsRequestATTState([MarshalAs(UnmanagedType.FunctionPtr)] Callback callback);

        [MonoPInvokeCallback(typeof(Callback))]
        private static void ATTRequestCallback(IntPtr _message)
        {
            string _status = Marshal.PtrToStringAuto(_message);

            if(att_request_response != null)
            {
                att_request_response(_status);
                att_request_response = null;
            }
        }

        [MonoPInvokeCallback(typeof(Callback))]
        private static void ATTStatusCallback(IntPtr _message)
        {
            string _status = Marshal.PtrToStringAuto(_message);

            if (att_status_response != null)
            {
                att_status_response(_status);
                att_status_response = null;
            }
        }
        #endif

        public static void Request(ATTRequestResponse _att_request_response)
        {
            att_request_response = _att_request_response;
            #if UNITY_IOS && !UNITY_EDITOR
                tutotoonsRequestATTAuth(ATTRequestCallback);
            #endif
        }

        public static void GetStatus(ATTStatusResponse _att_status_response)
        {
            att_status_response = _att_status_response;
            #if UNITY_IOS && !UNITY_EDITOR
                tutotoonsRequestATTState(ATTStatusCallback);
            #endif
        }

    }
}
