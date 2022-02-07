using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.Diagnostics;
using System;

public class iOSPostBuildProcessor
{
#if UNITY_IOS
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget _build_target, string _root_path)
    {
        if (_build_target == BuildTarget.iOS)
        {
            // MODIFYING INFO.PLIST
            string _info_plist_path = _root_path + "/Info.plist";
            PlistDocument _info_plist = new PlistDocument();
            _info_plist.ReadFromFile(_info_plist_path);
            PlistElementDict _root_dict = _info_plist.root;

            _root_dict.SetBoolean("GADIsAdManagerApp", true);

#if GEN_DEBUG
            _root_dict.SetBoolean("UIFileSharingEnabled", true);    
#endif

#if GEN_APP_TRACKING_TRANSPARENCY
            _root_dict.SetString("NSUserTrackingUsageDescription", TutoTOONS.ATTRequest.ATT_MESSAGE);
#endif
            
            // UIApplicationExitsOnSuspend is depracted, Unity 2019 still creates it, so we remove it
            // remove exit on suspend if it exists.
            string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
            if (_root_dict.values.ContainsKey(exitsOnSuspendKey))
            {
                _root_dict.values.Remove(exitsOnSuspendKey);
            }

            // Unity adds new capabilities to UIRequiredDeviceCapabilities and Apple doesn't allow to add new capabilities to existing apps.
            // Therefore we need to remove new Unity capabilities.
            string requiredDeviceCapabilities = "UIRequiredDeviceCapabilities";
            if(_root_dict.values.ContainsKey(requiredDeviceCapabilities))
            {
                _root_dict.values.Remove(requiredDeviceCapabilities);
            }

            File.WriteAllText(_info_plist_path, _info_plist.WriteToString());

            // MODIFYING PROJECT SETTINGS
            string _pbxproj_path = _root_path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            ProjectCapabilityManager _project_capabilities = new ProjectCapabilityManager(_pbxproj_path, "Unity-iPhone/mmk.entitlements", "Unity-iPhone");
            _project_capabilities.WriteToFile();
            
            // UGLY FIX FOR MISSING UNITYFRAMEWORK HEADER IN XCODE
            string _main_app_path = Path.Combine(_root_path, "MainApp", "main.mm");
            string _main_content = File.ReadAllText(_main_app_path);
            string _new_content = _main_content.Replace("#include <UnityFramework/UnityFramework.h>", @"#include ""../UnityFramework/UnityFramework.h""");
            File.WriteAllText(_main_app_path, _new_content);
        }
    }
    
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget _build_target, string _root_path)
    {
        if (_build_target == BuildTarget.iOS)
        {
            PBXProject _project = new PBXProject();
            string _project_path = PBXProject.GetPBXProjectPath(_root_path);
            _project.ReadFromFile(_project_path);

            string _target_guid = _project.GetUnityMainTargetGuid();
            string _target_framwork_guid = _project.GetUnityFrameworkTargetGuid();

            ModifyFrameworksSettings(_project, _target_guid, _root_path);
            InitCocoaPods(_project, _root_path);
            _project.SetBuildProperty(_target_guid, "USYM_UPLOAD_AUTH_TOKEN", "FakeToken");

            _project.SetBuildProperty(_target_framwork_guid, "USYM_UPLOAD_AUTH_TOKEN", "FakeToken");

#if GEN_IRONSOURCE_MEDIATION_FACEBOOK

            // Facebook frameworks
            _project.AddFrameworkToProject(_target_framwork_guid, "FBSDKCoreKit.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "FBSDKGamingServicesKit.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "FBSDKLoginKit.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "FBSDKShareKit.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "GameKit.framework", true);
#endif

            // Singular frameworks
            _project.AddFrameworkToProject(_target_framwork_guid, "libsqlite3.0.tbd", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "SystemConfiguration.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "Security.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "libz.tbd", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "AdSupport.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "iAd.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "WebKit.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "StoreKit.framework", true);
            _project.AddFrameworkToProject(_target_framwork_guid, "AdServices.framework", true);

#if GEN_APP_TRACKING_TRANSPARENCY
            _project.AddFrameworkToProject(_target_framwork_guid, "AppTrackingTransparency.framework", true);
            _project.AddBuildProperty(_target_guid, "GCC_PREPROCESSOR_DEFINITIONS", "APP_TRACKING_TRANSPARENCY");
            _project.AddBuildProperty(_target_framwork_guid, "GCC_PREPROCESSOR_DEFINITIONS", "APP_TRACKING_TRANSPARENCY");
#endif
            
            File.WriteAllText(_project_path, _project.WriteToString());
        }
    }

    [PostProcessBuildAttribute(int.MaxValue)]
    public static void OnPostProcessBuildEnd(BuildTarget _build_target, string _root_path)
    {
#if GEN_IS_GENERATOR || UNITY_CLOUD_BUILD
        return;
#endif

        try {
            Process proc = new Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \" chmod 777 " + _root_path + "/install.sh\ncd " + _root_path + "\n./install.sh\nexit\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
        }
        catch(Exception e) {
            UnityEngine.Debug.Log("Failed to initialize Cocoa Pods.");
        }
    }
    
    static void ModifyFrameworksSettings(PBXProject _project, string _target_guid, string _root_path)
    {
        // SuperAwesome Configuration
        _project.AddBuildProperty(_target_guid, "SWIFT_VERSION", "4.2");
        _project.AddBuildProperty(_target_guid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Plugins/iOS/SuperAwesome-Bridging-Header.h");
        _project.AddBuildProperty(_target_guid, "CLANG_ENABLE_MODULES", "YES");

        // Adding Firebase config file
        string _firebase_cofig_path = _root_path + "/Data/Raw/GoogleService-Info.plist";
        if (File.Exists(_firebase_cofig_path))
        {
            _project.AddFileToBuild(_target_guid, _project.AddFile(_firebase_cofig_path, "GoogleService-Info.plist"));
        }
    }
    
    static void InitCocoaPods(PBXProject _project, string _path)
    {
        File.WriteAllText(_path + "/Podfile", GetPodfileContent());
#if GEN_IS_GENERATOR || UNITY_CLOUD_BUILD
        return;
#endif

        File.WriteAllText(_path + "/install.sh", "#!/bin/sh\nosascript -e 'tell application \"Terminal\" to do script \"cd " + _path + "\n# Commands start\n# Commands end\nexit\"'\nexit");
        TutoToonsEditor.AddShellCommand(_path + "/install.sh", "pod install");
    }

    private static string GetPodfileContent()
    {
        string _content =
#if !UNITY_CLOUD_BUILD
            "platform :ios, '11.0'\n" +
#else
            "platform :ios, '10.0'\n" +
#endif
            "target 'UnityFramework' do\n" +
            "use_frameworks!\n" +
            "#Dependencies unity start\n" +
            "#Dependencies unity end\n" +
            "end\n" +
            "target 'Unity-iPhone' do\n" +
            "use_frameworks!\n" +
            "#Dependencies root start\n" +
            "#Dependencies root end\n" +
            "end\n" +
            "\n" +
            "post_install do | installer |\n" +
            "installer.pods_project.targets.each do | target |\n" +
            "if target.name == \"Pods-Unity-iPhone\"\n" +
            "target.build_configurations.each do | config |\n" +
            "xcconfig_path = config.base_configuration_reference.real_path\n" +
            "xcconfig_lines = File.readlines(xcconfig_path)\n" +
            "new_xcconfig = \"\"\n" +
            "xcconfig_lines.each do | line |\n" +
            "index_other_linker_flags = line.index(\"OTHER_LDFLAGS\")\n" +
            "if (index_other_linker_flags == 0)\n" +
            "new_xcconfig += \"OTHER_LDFLAGS\"\n" +
            "next\n" +
            "end\n" +
            "new_xcconfig += line\n" +
            "end\n" +
            "File.open(xcconfig_path, \"w\") { | file | file << new_xcconfig }\n" +
            "end\n" +
            "end\n" +
            "end\n" +
            "end\n";

        return _content;
    }
#endif
}
