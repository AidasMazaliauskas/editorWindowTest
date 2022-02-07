

//class for ../_server/_tutoInfo.json
using System;

[Serializable]
public class GeneratorTutoInfo
{
    public string server_id;
    public string title;
    public string process_number;
    public string air_version;
    public string unity_path;
    public FtpInfoOld ftp_info_old;
    public FtpInfo ftp_info;
    public Authentication authentication;
    public UnityAccess unity_access;
}
