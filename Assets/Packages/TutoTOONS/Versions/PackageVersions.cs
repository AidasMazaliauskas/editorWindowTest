using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class PackageVersions
{
    private class CallbackInfo
    {
        public object instance;
        public MethodInfo methodInfo;
    }

    private static List<CallbackInfo> packageVersionsCallbacks;

    public static void Init()
    {
        Type _packageAddCallbackInterfaceType = typeof(IOnPackageVersionAdd);

        packageVersionsCallbacks = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => _packageAddCallbackInterfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .Select(x => new CallbackInfo { instance = x.Assembly.CreateInstance(x.Name), methodInfo = x.GetMethod("OnPackageVersionAdd") })
            .ToList();

        LogAllVersionsForPackages();
    }

    private static void LogAllVersionsForPackages()
    {
        foreach (CallbackInfo _callback in packageVersionsCallbacks)
        {
            MethodInfo _method = _callback.methodInfo;
            _method.Invoke(_callback.instance, null);
        }
    }
}
