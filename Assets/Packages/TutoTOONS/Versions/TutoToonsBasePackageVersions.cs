using System.Collections.Generic;
using TutoTOONS.Utils.Debug.Console;

public class TutoToonsBasePackageVersions : IOnPackageVersionAdd
{
    private static readonly List<PackageVersion> packageVersions = new List<PackageVersion>()
    {
        // Generated package versions
        new PackageVersion("Tuto Toons Base Package", "1.2.9", new System.DateTime(2022, 1, 18)),

        // Manually inserted package versions
    };

    public void OnPackageVersionAdd()
    {
        DebugConsole.AddSDKVersion(packageVersions);
    }
}
