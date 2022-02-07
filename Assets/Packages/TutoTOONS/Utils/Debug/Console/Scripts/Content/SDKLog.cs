using static TutoTOONS.Utils.Debug.SDKDebug;

namespace TutoTOONS.Utils.Debug.Console
{
    public class SDKLog
    {
        public ServiceName service_name;
        public string message;

        public SDKLog(ServiceName _service_name, string _message)
        {
            service_name = _service_name;
            message = _message;
        }
    }
}
