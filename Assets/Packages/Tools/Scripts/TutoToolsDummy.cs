using System;

namespace TutoTOONS 
{
    public class TutoToolsDummy : ITutoToolsNativeBridge
    {
        public event Action<string> GotDeviceId;

        public void Init(string _event_object_name) { }
        public void SetDebugMode(bool _enabled) { }
        public string GetValue(string _key, string _default_value = "") { return string.Empty; }
        public void SetValue(string _key, string _value) { }
        public void RemoveValue(string _key) { }
        public bool ContainsDataInPackage(string _data) { return false; }
        public void RequestDeviceId() { }
        public void RequestDRM(string _public_key, string _uid, int _nonce) { }
    }
}