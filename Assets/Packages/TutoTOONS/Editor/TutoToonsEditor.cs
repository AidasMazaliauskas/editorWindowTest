using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Xml;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Linq;

#if GEN_PLATFORM_UDP
using UnityEngine.UDP.Editor;
#endif

public class TutoToonsEditor : AssetPostprocessor, IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
{
    public const string AD_NETWORK_KIDOZ = "kidoz";
    public const string AD_NETWORK_AD_MOB = "admob";
    public const string AD_NETWORK_APPLOVIN_MAX = "applovin";

    public const string AD_NETWORK_ADMOB_SKADNETWORK = "admob";
    public const string AD_NETWORK_APPLOVIN_SKADNETWORK = "applovin_max";
    public const string AD_NETWORK_IRONSOURCE_SKADNETWORK = "ironsource";


    public static string path_to_android_manifest = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";
    public static XmlDocument android_manifest_document;
    private static bool is_build_cleaned = false;
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
#if !GEN_IS_GENERATOR
        RemoveErrorMessage();
#endif
        RemovePrefab("IronSourceEvents");
        RemovePrefab("TutoAds");
        RemovePrefab("Kidoz");

#if GEN_SUBSCRIPTION
        TutoTOONS.SubscriptionEditor.GenerateIconList();
#endif

#if UNITY_IOS
        RunPostProcessTasksiOS();
#elif UNITY_ANDROID
        CleanBuildFiles();

        BuildArguments _build_arguments = BuildTools.GetBuildArguments();

        if (_build_arguments.safedk)
        {
            AddSafeDK();
        }

        RunPostProcessTasksAndroid();
#endif
        AddPrivacyPolicy(PlayerSettings.applicationIdentifier);
    }

    public static void AddPrivacyPolicy(string _bundle_id)
    {
        string _source = Path.Combine(Application.dataPath, "Packages/TutoTOONS/PrivacyPolicy/StreamingAssets");
        string _destination = Path.Combine(Application.dataPath, "StreamingAssets/PrivacyPolicy");
        if (Directory.Exists(_destination))
        {
            Directory.Delete(_destination, true);
        }
        CopyDirectoryRecursive(_source, _destination);

        string _html_file = Path.Combine(_destination, "PrivacyPolicyNoWifi.html");

        if (_bundle_id.IndexOf(TutoTOONS.AppConfig.BUNDLE_ID_TUTOTOONS) == 0)
        {
            replaceSymbolInFile(_html_file, "***PRIVACY_URL***", "https://tutotoons.com/privacy_policy/embeded");
        }
        else if (_bundle_id.IndexOf(TutoTOONS.AppConfig.BUNDLE_ID_CUTE_AND_TINY) == 0)
        {
            replaceSymbolInFile(_html_file, "***PRIVACY_URL***", "https://cutetinygames.com/privacy_policy/embedded");
        }
        else if (_bundle_id.IndexOf(TutoTOONS.AppConfig.BUNDLE_ID_SPINMASTER) == 0)
        {
            replaceSymbolInFile(_html_file, "***PRIVACY_URL***", "https://spinmaster.helpshift.com/a/mightyexpress/?p=web&s=privacy-policy&f=spin-master-privacy-policy&l=en");
        }
        else if (_bundle_id.IndexOf(TutoTOONS.AppConfig.BUNDLE_ID_SUGARFREE) == 0)
        {
            replaceSymbolInFile(_html_file, "***PRIVACY_URL***", "https://sugarfree.games/privacy.html");
        }
        else
        {
            replaceSymbolInFile(_html_file, "***PRIVACY_URL***", "https://tutotoons.com/privacy_policy/embeded");
        }
    }

    static void AddSafeDK()
    {
        string _url = "https://apps.tutotoons.com/";

        UnityWebRequest _www = UnityWebRequest.Get(_url + TutoTOONS.SystemUtils.GetBundleID() + ".android.json");
        _www.SendWebRequest();
        while (!_www.isDone)
        {
            //Wait
        }
#if UNITY_2020_1_OR_NEWER
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
#else
        if (_www.isHttpError || _www.isNetworkError)
#endif
        {
            Debug.Log("Error: Failed to get app config for safedk: " + _www.error);
            return;
        }

        string _result = _www.downloadHandler.text;

        TutoTOONS.AppConfigData _data = JsonUtility.FromJson<TutoTOONS.AppConfigData>(_result);

        string _content = File.ReadAllText(Application.dataPath + "/Plugins/Android/launcherTemplate.gradle");

        if (!_content.Contains("//SAFEDKADDED"))
        {
            string _template = File.ReadAllText(Application.dataPath + "/Packages/TutoTOONS/Editor/safedkgradle.txt");

            _template = _template.Replace("*APP_ID*", _data.settings.safedk_id);
            _template = _template.Replace("*APP_KEY*", _data.settings.safedk_key);

            string _keyword = "//SAFEDKSTART";

            string _new_content = _content;
            _new_content = _new_content.Insert(_new_content.IndexOf(_keyword) + _keyword.Length + 1, _template);

            File.WriteAllText(Application.dataPath + "/Plugins/Android/launcherTemplate.gradle", _new_content);
        }
        string _content_base = File.ReadAllText(Application.dataPath + "/Plugins/Android/baseProjectTemplate.gradle");

        if (!_content_base.Contains("//SAFEDKADDED"))
        {
            string _keyword1 = "//SAFEDK1";
            string _keyword2 = "//SAFEDK2";
            string _keyword3 = "//SAFEDK3";

            string _new_content_base = _content_base;
            _new_content_base = _new_content_base.Insert(_new_content_base.IndexOf(_keyword1) + _keyword1.Length + 1, "classpath \"com.safedk:SafeDKGradlePlugin:1.+\"");
            _new_content_base = _new_content_base.Insert(_new_content_base.IndexOf(_keyword2) + _keyword2.Length + 1, "maven { url 'http://download.safedk.com/maven' }");
            _new_content_base = _new_content_base.Insert(_new_content_base.IndexOf(_keyword3) + _keyword3.Length + 1, "maven { url 'http://download.safedk.com/maven' }");
            _new_content_base += "\n//SAFEDKADDED";
            File.WriteAllText(Application.dataPath + "/Plugins/Android/baseProjectTemplate.gradle", _new_content_base);
        }
    }

    private void CleanBuildFiles()
    {
        if (!is_build_cleaned)
        {
            string _path_clean_android_manifest = Path.Combine(Application.dataPath, "Packages/TutoTOONS/Editor/BuildResources/AndroidManifest.xml");
            string _path_clean_gradle_template = Path.Combine(Application.dataPath, "Packages/TutoTOONS/Editor/BuildResources/mainTemplate.gradle");
            string _path_clean_gradle_base_template = Path.Combine(Application.dataPath, "Packages/TutoTOONS/Editor/BuildResources/baseProjectTemplate.gradle");
            string _path_clean_gradle_launcher_template = Path.Combine(Application.dataPath, "Packages/TutoTOONS/Editor/BuildResources/launcherTemplate.gradle");
            string _path_clean_gradle_properties = Path.Combine(Application.dataPath, "Packages/TutoTOONS/Editor/BuildResources/gradleTemplate.properties");
            string _path_current_android_manifest = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
            string _path_current_gradle_template = Path.Combine(Application.dataPath, "Plugins/Android/mainTemplate.gradle");
            string _path_current_gradle_base_template = Path.Combine(Application.dataPath, "Plugins/Android/baseProjectTemplate.gradle");
            string _path_current_gradle_launcher_template = Path.Combine(Application.dataPath, "Plugins/Android/launcherTemplate.gradle");
            string _path_current_gradle_properties = Path.Combine(Application.dataPath, "Plugins/Android/gradleTemplate.properties");
            string _path_network_security_config = Path.Combine(Application.dataPath, "Plugins/Android/res/xml/network_security_config.xml");

            if (Directory.Exists(Path.Combine(Application.dataPath, "Plugins/Android/res")))
            {
                Directory.Delete(Path.Combine(Application.dataPath, "Plugins/Android/res"), true);
            }

            DeleteFile(_path_current_android_manifest);
            DeleteFile(_path_current_gradle_template);
            DeleteFile(_path_current_gradle_base_template);
            DeleteFile(_path_current_gradle_launcher_template);
            DeleteFile(_path_current_gradle_properties);
            DeleteFile(_path_network_security_config);
            CopyFile(_path_clean_android_manifest, _path_current_android_manifest);
            CopyFile(_path_clean_gradle_template, _path_current_gradle_template);
            CopyFile(_path_clean_gradle_base_template, _path_current_gradle_base_template);
            CopyFile(_path_clean_gradle_launcher_template, _path_current_gradle_launcher_template);
            CopyFile(_path_clean_gradle_properties, _path_current_gradle_properties);

            ProcessBuildFiles();

            // Temporary Amazon IAP manifest fix
            string _path_amazon_iap_sdk_new = Path.Combine(Application.dataPath, "Packages/TutoTOONS/Editor/BuildResources/AmazonAppStore.aar");
            string _path_amazon_iap_sdk = Path.Combine(Application.dataPath, "Plugins/UnityPurchasing/Bin/Android/AmazonAppStore.aar");
            if (File.Exists(_path_amazon_iap_sdk) && File.Exists(_path_amazon_iap_sdk_new))
            {
                DeleteFile(_path_amazon_iap_sdk);
                CopyFile(_path_amazon_iap_sdk_new, _path_amazon_iap_sdk);
            }
            // Temporary Amazon IAP manifest fix end

            ReadAndroidManifestDocument();

            is_build_cleaned = true;
        }
    }

    private static void ProcessBuildFiles()
    {
        string _path_gradle_template = Path.Combine(Application.dataPath, "Plugins/Android/mainTemplate.gradle");
        string _path_gradle_launcher_template = Path.Combine(Application.dataPath, "Plugins/Android/launcherTemplate.gradle");
        string _path_gradle_properties = Path.Combine(Application.dataPath, "Plugins/Android/gradleTemplate.properties");
        string _path_gradle_base_template = Path.Combine(Application.dataPath, "Plugins/Android/baseProjectTemplate.gradle");
        string _path_android_manifest = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

#if UNITY_2020_1_OR_NEWER
        replaceSymbolInFile(_path_gradle_template, "**TUTO_COMPRESSION_SUPPORT**", "noCompress = ['.ress', '.resource', '.obb'] + unityStreamingAssets.tokenize(', ')");
        replaceSymbolInFile(_path_gradle_launcher_template, "**TUTO_COMPRESSION_SUPPORT**", "noCompress = ['.ress', '.resource', '.obb'] + unityStreamingAssets.tokenize(', ')");
        replaceSymbolInFile(_path_gradle_launcher_template, "**TUTO_PROGUARD_DEBUG_SUPPORT**", "");
        replaceSymbolInFile(_path_gradle_launcher_template, "**TUTO_PROGUARD_RELEASE_SUPPORT**", "");
        replaceSymbolInFile(_path_gradle_properties, "**TUTO_UNITY_STREAMING_ASSETS**", "unityStreamingAssets =.unity3d**STREAMING_ASSETS**");
        replaceSymbolInFile(_path_gradle_base_template, "**TUTO_GRADLE_VERSION**", "3.6.0");

#if GEN_PLATFORM_AMAZON
        replaceSymbolInFile(_path_gradle_properties, "**TUTO_R8_COMPILER**", "android.enableR8=false");
#else
        replaceSymbolInFile(_path_gradle_properties, "**TUTO_R8_COMPILER**", "android.enableR8=**MINIFY_WITH_R_EIGHT**");
#endif
#else
            replaceSymbolInFile(_path_gradle_template, "**TUTO_COMPRESSION_SUPPORT**", "");
            replaceSymbolInFile(_path_gradle_launcher_template, "**TUTO_COMPRESSION_SUPPORT**", "noCompress = ['.unity3d', '.ress', '.resource', '.obb'**STREAMING_ASSETS**]");
            replaceSymbolInFile(_path_gradle_launcher_template, "**TUTO_PROGUARD_DEBUG_SUPPORT**", "useProguard **PROGUARD_DEBUG**");
            replaceSymbolInFile(_path_gradle_launcher_template, "**TUTO_PROGUARD_RELEASE_SUPPORT**", "useProguard **PROGUARD_RELEASE**");
            replaceSymbolInFile(_path_gradle_properties, "**TUTO_UNITY_STREAMING_ASSETS**", "");
            replaceSymbolInFile(_path_gradle_base_template, "**TUTO_GRADLE_VERSION**", "3.4.3");

#if GEN_PLATFORM_AMAZON
                replaceSymbolInFile(_path_gradle_properties, "**TUTO_R8_COMPILER**", "android.enableR8=false");
#else
                replaceSymbolInFile(_path_gradle_properties, "**TUTO_R8_COMPILER**", "");
#endif
#endif
    }

    private static void replaceSymbolInFile(string _file_path, string _symbol, string _replace_text)
    {
        StreamReader _stream_reader = new StreamReader(_file_path);
        string _file_contents = _stream_reader.ReadToEnd();
        _stream_reader.Close();
        string _updated_file_contents = _file_contents.Replace(_symbol, _replace_text);
        File.WriteAllText(_file_path, _updated_file_contents);
    }

    public static XmlElement GenerateXMLElement(string _name, Attribute[] _attributes)
    {
        XmlElement _activity_element = android_manifest_document.CreateElement(_name);

        for (int i = 0; i < _attributes.Length; i++)
        {
            _activity_element.SetAttribute("android__" + _attributes[i].name, _attributes[i].value);
        }

        return _activity_element;
    }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        is_build_cleaned = false;
    }

    public virtual void RunPostProcessTasksAndroid() { }

    public virtual void RunPostProcessTasksiOS() { }

    public static void CopyDirectoryRecursive(string _source_dir, string _target_dir)
    {
        DirectoryInfo _source_dir_info = new DirectoryInfo(_source_dir);
        DirectoryInfo _traget_dir_info = new DirectoryInfo(_target_dir);
        CopyAllFiles(_source_dir_info, _traget_dir_info);
    }

    public static void CopyAllFiles(DirectoryInfo _source_info, DirectoryInfo _target_info)
    {
        Directory.CreateDirectory(_target_info.FullName);

        foreach (FileInfo _file_info in _source_info.GetFiles())
        {
            if (_file_info.Name.EndsWith("meta"))
            {
                continue;
            }

            _file_info.CopyTo(Path.Combine(_target_info.FullName, _file_info.Name), true);
        }

        foreach (DirectoryInfo _sub_dir in _source_info.GetDirectories())
        {
            DirectoryInfo _target_sub_dir = _target_info.CreateSubdirectory(_sub_dir.Name);
            CopyAllFiles(_sub_dir, _target_sub_dir);
        }
    }

    public static void CopyFile(string path_source, string path_traget)
    {
        FileInfo _source_file = new FileInfo(path_source);
        _source_file.CopyTo(path_traget);
    }

    public static void DeleteFile(string path_file)
    {
        if (File.Exists(path_file))
        {
            File.Delete(path_file);
        }
    }

    public static void CleanManifestFile(string manifestPath)
    {
        TextReader _stream_reader = new StreamReader(manifestPath);
        string _file_content = _stream_reader.ReadToEnd();
        _stream_reader.Close();

        Regex _regex = new Regex("android__");
        _file_content = _regex.Replace(_file_content, "android:");

        TextWriter _stream_writer = new StreamWriter(manifestPath);
        _stream_writer.Write(_file_content);
        _stream_writer.Close();
    }

    public static void ReadAndroidManifestDocument()
    {
        if (android_manifest_document == null)
        {
            android_manifest_document = new XmlDocument();
            android_manifest_document.Load(path_to_android_manifest);
        }
    }

    public static void AddApplicationNodeToManifest(string _node_type, string _name, XmlElement _node_to_add)
    {
        XmlElement _manifest_root = android_manifest_document.DocumentElement;
        XmlNode _application_node = null;

        foreach (XmlNode _node in _manifest_root.ChildNodes)
        {
            if (_node.Name == "application")
            {
                _application_node = _node;
                break;
            }
        }

        if (_application_node == null)
        {
            Debug.LogError("There is no application node in AndroidManifest.xml");
            return;
        }

        List<XmlNode> _activity_nodes = GetNodes(_application_node, _node_type);

        bool _node_exists = false;

        for (int i = 0; i < _activity_nodes.Count; i++)
        {
            foreach (XmlAttribute _attribute in _activity_nodes[i].Attributes)
            {
                if (_attribute.Value.Contains(_name))
                {
                    _node_exists = true;
                }
            }
        }

        if (!_node_exists)
        {
            _application_node.AppendChild(_node_to_add);
        }

        android_manifest_document.Save(path_to_android_manifest);
        CleanManifestFile(path_to_android_manifest);
    }

    public static List<XmlNode> GetNodes(XmlNode _application_node, string _node_name)
    {
        List<XmlNode> _nodes = new List<XmlNode>();
        foreach (XmlNode _node in _application_node.ChildNodes)
        {
            if (_node.Name == _node_name)
            {
                _nodes.Add(_node);
            }
        }
        return _nodes;
    }

    public static void AddBuildScriptDependency(string _dependency)
    {
        string _path = Path.Combine(Application.dataPath, "Plugins/Android/baseProjectTemplate.gradle");
        string _regex = "(?s)(?<=\\/\\/ Dependencies start).*(?=\\/\\/ Dependencies end)";
        AppendTextIntoFile(_path, _regex, _dependency);
    }

    public static void AddBuildScriptRepository(string _dependency)
    {
        string _path = Path.Combine(Application.dataPath, "Plugins/Android/baseProjectTemplate.gradle");
        string _regex = "(?s)(?<=\\/\\/ Repositories start).*(?=\\/\\/ Repositories end)";
        AppendTextIntoFile(_path, _regex, _dependency);
    }

    public static void ApplyLauncherPlugins(string _plugin)
    {
        string _path = Path.Combine(Application.dataPath, "Plugins/Android/launcherTemplate.gradle");
        string _regex = "(?s)(?<=\\/\\/ Apply plugins start).*(?=\\/\\/ Apply plugins end)";
        AppendTextIntoFile(_path, _regex, _plugin);
    }

    public static void AddLauncherDependeny(string _dependency)
    {
        string _path = Path.Combine(Application.dataPath, "Plugins/Android/launcherTemplate.gradle");
        string _regex = "(?s)(?<=\\/\\/ Dependencies start).*(?=\\/\\/ Dependencies end)";
        AppendTextIntoFile(_path, _regex, _dependency);
    }
    public static void AddShellCommand(string _path, string _command)
    {
        string _regex = "(?s)(?<=# Commands start).*(?=# Commands end)";
        AppendTextIntoFile(_path, _regex, _command + "\n");
    }

    public static void AddGradleDependeny(string _dependency, string _conflicting_dependency = null)
    {
        string _path = Path.Combine(Application.dataPath, "Plugins/Android/mainTemplate.gradle");
        string _regex = "(?s)(?<=\\/\\/ Dependencies start).*(?=\\/\\/ Dependencies end)";
        if (ShouldAppendDependency(_path, _regex, _dependency, "// Dependencies start"))
        {
            AppendTextIntoFile(_path, _regex, _dependency, _conflicting_dependency);
        }
    }

    private static bool ShouldAppendDependency(string _file_path, string _regex_dependency_block, string _incoming_dependency, string _insert_keyword)
    {
        string _file_content = ReadFileToEnd(_file_path);
        Regex _regex = new Regex(_regex_dependency_block);
        Match _regex_matches = _regex.Match(_file_content);
        string _matches_block = _regex_matches.Value;
        string _incoming_dependency_string = ParseGradleDependencyString(_incoming_dependency);
        string _current_dependency_string = _matches_block.Split('\n').FirstOrDefault(x => x.Contains(_incoming_dependency_string));

        if (!string.IsNullOrEmpty(_current_dependency_string))
        {
            Version _incoming_dependency_version = ParseGradleDependencyVersion(_incoming_dependency);
            Version _current_dependency_version = ParseGradleDependencyVersion(_current_dependency_string);

            if (_incoming_dependency_version <= _current_dependency_version)
            {
                return false;
            }

            _file_content = ReplaceInBlock(_file_content, _current_dependency_string, string.Empty, _insert_keyword);
            WriteToFile(_file_path, _file_content);
        }

        return true;
    }

#if UNITY_IOS
    private static string UpdatePodDependencyBlock(string _podfile_content, string _incoming_dependency, Regex _regex_dependency_block, string _insert_keyword)
    {
        string _incoming_dependency_string = ParsePodDependencyString(_incoming_dependency);
        Version _incoming_dependency_version = ParsePodDependencyVersion(_incoming_dependency);

        string _dependencies = _regex_dependency_block.Match(_podfile_content).Value;
        string _updated_file_content = ShouldInsertPodDependency(_dependencies, _incoming_dependency_string, _incoming_dependency_version, _podfile_content, _insert_keyword, out bool _shouldInsertDependency);

        if (_shouldInsertDependency)
        {
            _updated_file_content = InsertDependency(_updated_file_content, _insert_keyword, _incoming_dependency);
        }

        return _updated_file_content;
    }

    private static string ShouldInsertPodDependency(string _dependency_block, string _dependency_keyword, Version _incoming_dependency_version, string _file_content, string _insert_keyword, out bool _shouldInsertDependency)
    {
        string _current_dependency_string = _dependency_block.Split('\n').FirstOrDefault(x => x.Contains(_dependency_keyword));
        _shouldInsertDependency = false;

        if (!string.IsNullOrEmpty(_current_dependency_string))
        {
            Version _current_dependency_version = ParsePodDependencyVersion(_current_dependency_string);

            if (_incoming_dependency_version > _current_dependency_version)
            {
                _shouldInsertDependency = true;
                return ReplaceInBlock(_file_content, _current_dependency_string, string.Empty, _insert_keyword);
            }

            return _file_content;
        }

        _shouldInsertDependency = true;
        return _file_content;
    }
#endif

    public static void AddCocoaPodsDependeny(string _root_path, string _dependency, bool _add_target_root = true, bool _add_target_unity = true)
    {
#if UNITY_IOS
        string _podfile_content = ReadFileToEnd(_root_path + "/Podfile");
        if (_add_target_root)
        {
            string _root_block_insert_keyword = "#Dependencies root start";
            Regex _regex_root_block = new Regex("(?s)(?<=#Dependencies root start).*(?=#Dependencies root end)");

            _podfile_content = UpdatePodDependencyBlock(_podfile_content, _dependency, _regex_root_block, _root_block_insert_keyword);
        }

        if (_add_target_unity)
        {
            string _unity_block_insert_keyword = "#Dependencies unity start";
            Regex _regex_unity_block = new Regex("(?s)(?<=#Dependencies unity start).*(?=#Dependencies unity end)");

            _podfile_content = UpdatePodDependencyBlock(_podfile_content, _dependency, _regex_unity_block, _unity_block_insert_keyword);
        }

        WriteToFile(_root_path + "/Podfile", _podfile_content);
#endif
    }

    private static string ParseGradleDependencyString(string _dependency)
    {
        string[] _chars = new[] { "implementation", " ", ",", "\'", "’", "‘", "\"" };

        _dependency = _dependency.Substring(0, _dependency.LastIndexOf(':'));
        foreach (string _c in _chars)
        {
            _dependency = _dependency.Replace(_c, string.Empty);
        }

        return _dependency;
    }

    private static Version ParseGradleDependencyVersion(string _dependency)
    {
        Regex _versionRegex = new Regex("(?<=:)(\\d+\\.?)+");
        return new Version(_versionRegex.Match(_dependency).Value);
    }

#if UNITY_IOS
    private static Version ParsePodDependencyVersion(string _dependency)
    {
        Regex _regex = new Regex("(?<='|‘|‘~> |'~> )[0-9]+.*(?=’|')");
        return new Version(_regex.Match(_dependency).Value);
    }

    private static string ParsePodDependencyString(string _dependency)
    {
        string[] _chars = new[] { "pod", " ", ",", ".", "\'", "’", "‘" };

        _dependency = _dependency.Substring(0, _dependency.LastIndexOf(',') - 1);
        foreach (string _c in _chars)
        {
            _dependency = _dependency.Replace(_c, string.Empty);
        }

        return _dependency;
    }
#endif

    private static string ReplaceInBlock(string _file_content, string _old_value, string _new_value, string _insert_keyword)
    {
        int _index_of_block_start = _file_content.IndexOf(_insert_keyword);
        int _index_of_line_to_replace = _file_content.IndexOf(_old_value, _index_of_block_start);
        return _file_content.Substring(0, _index_of_line_to_replace) + _new_value + _file_content.Substring(_index_of_line_to_replace + _old_value.Length);
    }

    private static string ReadFileToEnd(string _file_path)
    {
        using TextReader _file_reader = new StreamReader(_file_path);
        return _file_reader.ReadToEnd();
    }

    private static void WriteToFile(string _file_path, string _file_content)
    {
        using TextWriter _pofile_writer = new StreamWriter(_file_path);
        _pofile_writer.Write(_file_content);
    }


    private static void AppendTextIntoFile(string _file_path, string _regex_string, string _text, string _conflict_text = null)
    {
        string _file_content = ReadFileToEnd(_file_path);
        Regex _regex = new Regex(_regex_string);
        Match _regex_matches = _regex.Match(_file_content);
        string _matches_block = _regex_matches.Value;

        if (_conflict_text == null)
        {
            _matches_block += "\n" + _text;
        }
        else
        {
            if (_matches_block.IndexOf(_conflict_text) == -1)
            {
                _matches_block += "\n" + _text;
            }
        }

        _file_content = _regex.Replace(_file_content, _matches_block);
        WriteToFile(_file_path, _file_content);
    }


    private static string InsertDependency(string _file_content, string _keyword, string _dependency)
    {
        var _index = _file_content.IndexOf(_keyword);
        _index += _keyword.Length + 1;
        while (_index >= 0)
        {
            var _val = _dependency + "\n";
            _file_content = _file_content.Insert(_index, _val);
            _index += _val.Length;
            _index = _file_content.IndexOf(_keyword, _index);
            if (_index < 0) break;
            _index += _keyword.Length + 1;
        }
        return _file_content;
    }

    public static void RemovePrefab(string _name_prefab_to_remove)
    {
        string _path_tutotoons_prefab = "Assets/Packages/TutoTOONS/TutoTOONS.prefab";
        GameObject _tutotoons = PrefabUtility.LoadPrefabContents(_path_tutotoons_prefab);

        for (int i = 0; i < _tutotoons.GetComponent<Transform>().childCount; i++)
        {
            if (_tutotoons.GetComponent<Transform>().GetChild(i).name.Equals(_name_prefab_to_remove))
            {
                _tutotoons.GetComponent<Transform>().GetChild(i).parent = null;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(_tutotoons, _path_tutotoons_prefab);
        PrefabUtility.UnloadPrefabContents(_tutotoons);
    }

    public static void AddPrefab(string _path_to_prefab)
    {
        string _path_tutotoons_prefab = "Assets/Packages/TutoTOONS/TutoTOONS.prefab";
        GameObject _tutotoons = PrefabUtility.LoadPrefabContents(_path_tutotoons_prefab);
        GameObject _prefab_to_add = PrefabUtility.LoadPrefabContents(_path_to_prefab);
        _prefab_to_add.GetComponent<Transform>().parent = _tutotoons.GetComponent<Transform>();
        PrefabUtility.RecordPrefabInstancePropertyModifications(_prefab_to_add.GetComponent<Transform>());
        PrefabUtility.SaveAsPrefabAsset(_tutotoons, _path_tutotoons_prefab);

        PrefabUtility.UnloadPrefabContents(_tutotoons);
    }

    public static SkadNetworkData GetSkadNetworkIds(string _mediation_keyword, int _attempts = 5)
    {
        string _url = "https://api.tutotoons.com/generatorWebService/mediation/get/" + _mediation_keyword;

        SkadNetworkData _result = new SkadNetworkData();
        string _tuto_info = File.ReadAllText(BuildTools.path_to_account);
        GeneratorTutoInfo root = JsonUtility.FromJson<GeneratorTutoInfo>(_tuto_info);

        UnityWebRequest _www = UnityWebRequest.Get(_url);
        _www.SetRequestHeader("X-Email", root.authentication.email);
        _www.SetRequestHeader("X-Password", root.authentication.password);
        _www.SendWebRequest();
        while (!_www.isDone)
        {
            //Wait
        }
#if UNITY_2020_1_OR_NEWER
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
#else
        if (_www.isHttpError || _www.isNetworkError)
#endif
        {
            if (_attempts > 0)
            {
                _result = GetSkadNetworkIds(_mediation_keyword, _attempts: _attempts - 1);
            }
            else
            {
                DisplayError($"Could not download skadnetwork ids for {_mediation_keyword} network");
                Debug.Log("Error: Failed to get " + _mediation_keyword + " keys: " + _www.error);
                return _result;
            }
        }
        _result = JsonUtility.FromJson<SkadNetworkData>(_www.downloadHandler.text);
        if (_result == null)
        {
            if (_attempts > 0)
            {
                _result = GetSkadNetworkIds(_mediation_keyword, _attempts: _attempts - 1);
            }
            else
            {
                DisplayError($"Could not parse skadnetwork ids for {_mediation_keyword} network");
                Debug.Log("Error: Failed to parse json " + _mediation_keyword + " keys from text");
                return _result;
            }
        }
        return _result;
    }


    public static string GetFacebookAppId(int _attempts = 5)
    {
        string _url = "https://apps.tutotoons.com/";
        string _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_ANDROID;
        TutoTOONS.AppConfigData _data;
#if UNITY_IOS
        _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_IOS;
#endif

#if GEN_PLATFORM_AMAZON
        _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_AMAZON;
#endif
        string _app_id = "";
        UnityWebRequest _www = UnityWebRequest.Get(_url + TutoTOONS.SystemUtils.GetBundleID() + "." + _platform_name + ".json");
        _www.SendWebRequest();
        while (!_www.isDone)
        {
            //waiting
        }
#if UNITY_2020_1_OR_NEWER
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
#else
        if (_www.isHttpError || _www.isNetworkError)
#endif
        {
            if (_attempts > 0)
            {
                _app_id = GetFacebookAppId(_attempts - 1);
            }
            else
            {
                DisplayError("Could not download facebook app id");
                return _app_id;
            }
        }

        string _result = _www.downloadHandler.text;

        try
        {
            _data = JsonUtility.FromJson<TutoTOONS.AppConfigData>(_result);
        }
        catch (Exception e)
        {
            return _app_id;
        }

        _app_id = _data.settings.fb_app_id;
        if (string.IsNullOrEmpty(_app_id))
        {
            if (_attempts > 0)
            {
                _app_id = GetFacebookAppId(_attempts - 1);
            }
            else
            {
                _app_id = "";
                DisplayError("Could not download facebook app id");
            }
        }

        return _app_id;
    }

    public static string GetAppKey(int _attempts = 5)
    {
        string _url = "https://apps.tutotoons.com/";

        string _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_ANDROID;

#if UNITY_IOS
        _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_IOS;
#endif

#if GEN_PLATFORM_AMAZON
        _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_AMAZON;
#endif

#if GEN_PLATFORM_UDP
        _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_UDP;
#endif
        // this method is called way before build, so we need to use bundle id from command line
        string _app_key = "";
        TutoTOONS.AppConfigData _data;
        string _bundle_id = BuildTools.GetBuildArguments().bundle_id;
        UnityWebRequest _www = UnityWebRequest.Get(_url + TutoTOONS.SystemUtils.GetBundleID(_bundle_id) + "." + _platform_name + ".json");
        _www.SendWebRequest();
        while (!_www.isDone)
        {
            //Wait
        }
#if UNITY_2020_1_OR_NEWER
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
#else
        if (_www.isHttpError || _www.isNetworkError)
#endif
        {
            if (_attempts > 0)
            {
                _app_key = GetAppKey(_attempts - 1);
            }
            else
            {
                Debug.Log("Error: Failed to get app key");
                return _app_key;
            }
        }

        string _result = _www.downloadHandler.text;

        try
        {
            _data = JsonUtility.FromJson<TutoTOONS.AppConfigData>(_result);
        }
        catch (Exception e)
        {
            return _app_key;
        }
        _app_key = _data.settings.app_key;
        if (string.IsNullOrEmpty(_app_key))
        {
            if (_attempts > 0)
            {
                _app_key = GetAppKey(_attempts - 1);
            }
            else
            {
                _app_key = "";
                DisplayError("Could not download App Key id");
            }
        }
        return _app_key;
    }

    public static AdNetworkKeys GetAdNetworkKeys(string _ad_network, int _attempts = 5)
    {
        string _url = "https://apps.tutotoons.com/";

        string _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_ANDROID;

#if UNITY_IOS
        _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_IOS;
#endif

#if GEN_PLATFORM_AMAZON
        _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_AMAZON;
#endif

#if GEN_PLATFORM_UDP
        _platform_name = TutoTOONS.AppConfig.PLATFORM_NAME_UDP;
#endif

        AdNetworkKeys _keys = new AdNetworkKeys();
        TutoTOONS.AppConfigData _data;
        UnityWebRequest _www = UnityWebRequest.Get(_url + TutoTOONS.SystemUtils.GetBundleID() + "." + _platform_name + ".json");
        _www.SendWebRequest();
        while (!_www.isDone)
        {
            //Wait
        }
#if UNITY_2020_1_OR_NEWER
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
#else
        if (_www.isHttpError || _www.isNetworkError)
#endif
        {
            if (_attempts > 0)
            {
                _keys = GetAdNetworkKeys(_ad_network, _attempts - 1);
            }
            else
            {
                DisplayKeysErrorMessage(_ad_network);
                Debug.Log("Error: Failed to get " + _ad_network + " keys: " + _www.error);
                return _keys;
            }
        }

        string _result = _www.downloadHandler.text;

        try
        {
            _data = JsonUtility.FromJson<TutoTOONS.AppConfigData>(_result);
        }
        catch (Exception e)
        {
            return _keys;
        }

        for (int i = 0; i < _data.ad_networks.Count; i++)
        {
            if (_data.ad_networks[i].keyword == _ad_network)
            {
                _keys.key1 = _data.ad_networks[i].settings.key1;
                _keys.key2 = _data.ad_networks[i].settings.key2;
                _keys.key3 = _data.ad_networks[i].settings.key3;

                _keys.key1_no_comp = _data.ad_networks[i].settings.key1_no_comp;
                _keys.key2_no_comp = _data.ad_networks[i].settings.key2_no_comp;
                _keys.key3_no_comp = _data.ad_networks[i].settings.key3_no_comp;

                break;
            }
        }

        if (_keys.key1.Length == 0 || _keys.key1 == "key_not_found" &&
            _keys.key2.Length == 0 || _keys.key2 == "key_not_found" &&
            _keys.key3.Length == 0 || _keys.key3 == "key_not_found" &&
            _keys.key1_no_comp.Length == 0 || _keys.key1_no_comp == "key_not_found" &&
            _keys.key2_no_comp.Length == 0 || _keys.key2_no_comp == "key_not_found" &&
            _keys.key3_no_comp.Length == 0 || _keys.key3_no_comp == "key_not_found")
        {
            if (_attempts > 0)
            {
                _keys = GetAdNetworkKeys(_ad_network, _attempts - 1);
            }
            else
            {
                DisplayKeysErrorMessage(_ad_network);
                Debug.Log("Error: " + _ad_network + " keys were not loaded");
            }
        }

        return _keys;
    }

    private static void RemoveErrorMessage()
    {
        string _tutotoons_path = $"Assets/Packages/TutoTOONS/TutoTOONS.prefab";
        GameObject _tuto_prefab = PrefabUtility.LoadPrefabContents(_tutotoons_path);
        Transform obj = _tuto_prefab.transform.Find("DebugConsole").Find("Canvas").Find("AdKeysError");
        if (obj == null)
        {
            return;
        }
        UnityEngine.Object.DestroyImmediate(obj.gameObject);
        PrefabUtility.SaveAsPrefabAsset(_tuto_prefab, _tutotoons_path);
        PrefabUtility.UnloadPrefabContents(_tuto_prefab);

    }

    protected static void DisplayKeysErrorMessage(string _ad_network)
    {
        DisplayError($"Failed to load {_ad_network} keys from app config. Please rebuild the game");
    }

    protected static void DisplayError(string _message)
    {
        string _asset_path = "Assets/Packages/TutoTOONS/TutoTOONS.prefab";
        GameObject _prefab_tuto = PrefabUtility.LoadPrefabContents(_asset_path);
        Text _text_comp;
        Transform _canvas = _prefab_tuto.transform.Find("DebugConsole").Find("Canvas");
        Transform _errorText = _canvas.Find("AdKeysError");

        if (_errorText != null)
        {
            _text_comp = _errorText.GetComponent<Text>();
        }
        else
        {
            GameObject _textobject = new GameObject($"AdKeysError");
            _textobject.transform.parent = _canvas;
            float _scale = _textobject.transform.localScale.x;
            _text_comp = _textobject.AddComponent<Text>();
            _text_comp.color = Color.red;
            _text_comp.fontSize = (int)(20 * _scale);
            if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait)
            {
                _text_comp.fontSize = 20;
            }
            _text_comp.transform.localScale = Vector3.one;
            _text_comp.alignment = TextAnchor.MiddleCenter;
            // stretch to canvas            
            RectTransform rt = _text_comp.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            _text_comp.transform.localPosition = new Vector3(0, 0, 1);
        }

        _text_comp.text += _message + "\n";

        PrefabUtility.SaveAsPrefabAsset(_prefab_tuto, _asset_path);
        PrefabUtility.UnloadPrefabContents(_prefab_tuto);
    }

    public static void AddIAPProductToCatalog(IAPConfig _iap_config, UDPConfig _udp_config)
    {
#if GEN_PLATFORM_UDP
        bool _already_exists = false;
        string _item_slug = "";

        // For some games purchases got messed on upd and we need to create purchases with new sku's
        if (ShouldUsePurchasesCheatUDP(TutoTOONS.SystemUtils.GetBundleID()))
        {
            // Modifying default purchase sku
            _item_slug = (TutoTOONS.SystemUtils.GetBundleID() + "._" + _iap_config.bundle_id_suffix).ToLower();
        }
        else
        {
            // creating IAP without any cheats
            _item_slug = (TutoTOONS.SystemUtils.GetBundleID() + "." + _iap_config.bundle_id_suffix).ToLower();
        }

        var _iap_item = new UnityEngine.UDP.Editor.IapItem();
        foreach (var item in _udp_config.iapItems)
        {
            if (item.slug == _item_slug)
            {
                _iap_item = item;
                _already_exists = true;
                break;
            }
        }

        List<UnityEngine.UDP.Editor.PriceDetail> _prices = new List<UnityEngine.UDP.Editor.PriceDetail>();
        _prices.Add(new UnityEngine.UDP.Editor.PriceDetail() { currency = "USD", price = _iap_config.price_tier });


        _iap_item.masterItemSlug = _udp_config.appItem.slug;
        _iap_item.slug = _item_slug;
        _iap_item.name = _iap_config.title;
        if (_iap_item.name.Length == 0)
        {
            _iap_item.name = _iap_config.bundle_id_suffix;
        }
        _iap_item.properties.description = _iap_config.description;
        if (_iap_item.properties.description.Length == 0)
        {
            _iap_item.properties.description = _iap_config.bundle_id_suffix;
        }
        _iap_item.consumable = _iap_config.is_consumable;
        _iap_item.priceSets = new UnityEngine.UDP.Editor.PriceSets()
        {
            PurchaseFee = new UnityEngine.UDP.Editor.PurchaseFee()
            {
                priceMap = new UnityEngine.UDP.Editor.PriceMap()
                {
                    DEFAULT = _prices
                }
            }
        };

        UnityWebRequest _www = AppStoreOnBoardApi.CreateStoreItem(_iap_item);
        if (_already_exists)
        {
            _www = AppStoreOnBoardApi.UpdateStoreItem(_iap_item);
        }

        while (!_www.isDone)
        {
            //wait till done
        }
#if UNITY_2020_1_OR_NEWER
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
#else
        if (_www.isHttpError || _www.isNetworkError)
#endif
        {
            Debug.Log("Error while creating item (item slug: " + _iap_item.slug + " ): " + _www.downloadHandler.text);
        }
#endif
    }

    private static bool ShouldUsePurchasesCheatUDP(string _bundle_id)
    {
        if (_bundle_id == "com.tutotoons.app.rockstaranimalhairsalon.free")
        {
            return true;
        }

        return false;
    }

    public static void CreateTestAccounts(List<TestAccount> _accounts, UDPConfig _config)
    {
#if GEN_PLATFORM_UDP
        FullUpdatePayload _full_payload = new FullUpdatePayload();

        _full_payload.clientId = _config.appItem.clientId;
        _full_payload.gameTitle = _config.appItem.name;

        _full_payload.testAccounts = new SimplePlayerPayload[_accounts.Count];
        for (int i = 0; i < _accounts.Count; i++)
        {
            _full_payload.testAccounts[i] = new SimplePlayerPayload()
            {
                email = _accounts[i].name,
                password = _accounts[i].password
            };
        }

        var _www = UnityEngine.UDP.Editor.AppStoreOnBoardApi.UpdateAll(_full_payload);
        while (!_www.isDone)
        {

        }
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error while creating test account: " + _www.error);
        }
#endif
    }

    public static List<IAPConfig> GetIAPConfig()
    {
        string _production_app_id = TutoTOONS.AppConfig.production_app_id;
        string _url = "https://api.tutotoons.com/generatorWebService/get/in_app_purchases/"
            + _production_app_id + "?testing_admin";
        UnityWebRequest _www = UnityWebRequest.Get(_url);
        _www.SendWebRequest();
        while (!_www.isDone)
        {
            //Wait
        }
#if UNITY_2020_1_OR_NEWER
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
#else
        if (_www.isHttpError || _www.isNetworkError)
#endif
        {
            Debug.Log("Error: Failed to get IAP config: " + _www.error);
            return null;
        }
        string _result = _www.downloadHandler.text;
        IAPConfigs _config = JsonUtility.FromJson<IAPConfigs>("{\"items\":" + _result + "}");
        return _config.items;
    }

    public static UDPConfig GetUDPConfig()
    {
#if GEN_PLATFORM_UDP
        UnityWebRequest _www = AppStoreOnBoardApi.FetchData();
        while (!_www.isDone)
        {
        }
#if UNITY_2020_1_OR_NEWER
        if (_www.result == UnityWebRequest.Result.ConnectionError || _www.result == UnityWebRequest.Result.ProtocolError)
#else
        if (_www.isHttpError || _www.isNetworkError)
#endif
        {
            Debug.Log("Error: " + _www.error);
        }
        UDPConfig _config = JsonUtility.FromJson<UDPConfig>(_www.downloadHandler.text);
        return _config;
#else
        return new UDPConfig();
#endif
    }


}
