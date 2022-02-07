using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ErrorHandler : MonoBehaviour
{
    static ErrorHandler()
    {
        if (Application.isBatchMode)
        {
            Application.logMessageReceived += HandleLog;
        }
    }

    static void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            string _message = "<<ERROR>>\n\n";
            _message += condition;
            _message += stackTrace;
            _message += "\n\n<<ERROR-END>>\n";
            Debug.Log(_message);
        }        
    }
}
