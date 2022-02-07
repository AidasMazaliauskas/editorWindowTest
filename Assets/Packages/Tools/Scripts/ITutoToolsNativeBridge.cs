using System;

public interface ITutoToolsNativeBridge
{
    void Init(string _event_object_name);
    void SetDebugMode(bool _enabled);
    void SetValue(string _key, string _value);
    string GetValue(string _key, string _default_value = "");
    void RemoveValue(string _key);
    bool ContainsDataInPackage(string _data);
    void RequestDeviceId();
    void RequestDRM(string _public_key, string _uid, int _nonce);
}
