
// Android keystore data
public class KeystoreData
{
    public string keystoreName;
    public string keystorePassword;
    public string keyAliasName;
    public string keyAliasPassword;

    public KeystoreData(string _keystore_data)
    {
        string[] _data = _keystore_data.Split(';');

        keystoreName = _data[0];
        keystorePassword = _data[1];
        keyAliasName = _data[2];
        keyAliasPassword = _data[3];
    }
}
