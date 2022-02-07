using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PackageExporter : EditorWindow
{
    #region Constants
    private const double TIMEOUT_SECONDS_CHECK_IF_PACKAGE_EXPORTED = 60;
    private const string EDITOR_WINDOW_NAME = "Export Packages";
    private const float TINY_SPACE = 1;
    private const float SMALL_SPACE = 5;
    private const float MEDIUM_SPACE = 10;
    private const float LARGE_SPACE = 20; 
    private const string VERSION_TITLE_LABEL = "Version";
    private const string RELEASE_NOTES_TITLE_LABEL = "Release Notes";
    private const string DEVELOPMENT_LABEL = "Development";
    #endregion

    [SerializeField] private bool isDevelopment = true;
    [SerializeField] private bool isExporting;
    [SerializeField] private bool finishedExporting = false;
    [SerializeField] private static string versionText;
    [SerializeField] private List<string> releaseNotes = new List<string>();
    [SerializeField] private Vector2 noteScrollPosition = new Vector2(0, 0);
    [SerializeField] private static bool styleInitialized = false;
    [SerializeField] private static int selectedItemPopupIndex;
    [SerializeField] private static List<string> availablePackages = new List<string>();
    [SerializeField] private string errorMessage = string.Empty;
    [SerializeField] private string successMessage = string.Empty;
    [SerializeField] private string informationMessage = string.Empty;
    [SerializeField] private int previousSelectedPackageIndex = -1;

    #region Editor Window Styles
    [SerializeField] private static GUIStyle resolutionNotesTitleStyle;
    [SerializeField] private static GUIStyle textAreaStyle;
    [SerializeField] private static GUIStyle removeNoteButtonStyle;
    [SerializeField] private static GUIStyle exportButtonStyle;
    [SerializeField] private static GUIStyle noteHorizontalLayoutStyle;
    #endregion

    [SerializeField]
    static readonly Dictionary<string, PackageData> packages = new Dictionary<string, PackageData>() 
    {
        { "AdMobMediation",         new PackageData() { paths = new List<string>(){ @"/Packages/AdMobMediation" } } },
        { "ApplovinMAX",            new PackageData() { paths = new List<string>(){ @"/Packages/ApplovinMAX"} } },
        { "BuilderPackage",         new PackageData() { paths = new List<string>(){ @"/Packages/Builder" } } },
        { "Chartboost",             new PackageData() { paths = new List<string>(){ @"/Packages/Chartboost" } } },
        { "FacebookPackage",        new PackageData() { paths = new List<string>(){ @"/Packages/FacebookPackage" } } },
        { "FirebaseAnalytics",      new PackageData() { paths = new List<string>(){ @"/Packages/FirebaseAnalytics", @"/Packages/Firebase"} } },
        { "FirebaseAuth",           new PackageData() { paths = new List<string>(){ @"/Packages/FirebaseAuth", @"/Packages/Firebase" } } },
        { "FirebaseCrashlytics",    new PackageData() { paths = new List<string>(){ @"/Packages/FirebaseCrashlytics", @"/Packages/Firebase" } } },
        { "FirebaseMessaging",      new PackageData() { paths = new List<string>(){ @"/Packages/FirebaseMessaging", @"/Packages/Firebase" } } },
        { "FirebaseRealtimeDatabase", new PackageData() { paths = new List<string>(){ @"/Packages/FirebaseDatabase", @"/Packages/Firebase" } } },
        { "Freetime",               new PackageData() { paths = new List<string>(){ @"/Packages/Freetime" } } },
        { "HuaweiAds",              new PackageData() { paths = new List<string>(){ @"/Packages/HuaweiAds", @"/Packages/Huawei" } } },
        { "HuaweiIAP",              new PackageData() { paths = new List<string>(){ @"/Packages/HuaweiIAP", @"/Packages/Huawei" } } },
        { "IronSourceMediation",    new PackageData() { paths = new List<string>(){ @"/Packages/IronSourceMediation" } } },
        { "Kidoz",                  new PackageData() { paths = new List<string>(){ @"/Packages/Kidoz" } } },
        { "Purchasing",             new PackageData() { paths = new List<string>(){ @"/Packages/Purchasing" } } },
        { "Singular",               new PackageData() { paths = new List<string>(){ @"/Packages/Singular" } } },
        { "Soomla",                 new PackageData() { paths = new List<string>(){ @"/Packages/Soomla" } } },
        { "SuperAwesome",           new PackageData() { paths = new List<string>(){ @"/Packages/SuperAwesome" } } },
        { "TutoAds",                new PackageData() { paths = new List<string>(){ @"/Packages/TutoAds" } } },
        { "TutoToonsBase",          new PackageData() { paths = new List<string>(){ @"/Packages/TutoTOONS", @"/Packages/InAppBrowser", @"/Plugins", @"/Packages/Tools" } } },
        { "TutoToonsIntroAnima2D",   new PackageData() { paths = new List<string>(){ @"/Packages/TutoTOONSIntro" } } },
        { "TutoToonsIntroSpine",    new PackageData() { paths = new List<string>(){ @"/Packages/TutoTOONSIntro" } } },
        { "TutoToonsIntroSpineV4",    new PackageData() { paths = new List<string>(){ @"/Packages/TutoTOONSIntro" } } },
        { "TutoToonsSubscription",  new PackageData() { paths = new List<string>(){ @"/Packages/TutoToonsSubscription", @"/PackagesCustom" } } },
    };
   
    [MenuItem("TutoTOONS/Export Packages")]
    private static void Open()
    {
        UpdateAvailablePackages();

        PackageExporter _window = (PackageExporter)GetWindow(typeof(PackageExporter), false, EDITOR_WINDOW_NAME);

        _window.position = new Rect(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(570, 240));
        _window.ShowUtility();
    }

    private void OnGUI()
    {
        if (!styleInitialized)
        {
            InitializeStyles();
        }

        EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
        {
            selectedItemPopupIndex = EditorGUILayout.Popup(selectedItemPopupIndex, availablePackages.ToArray());

            if(previousSelectedPackageIndex != selectedItemPopupIndex)
            {
                previousSelectedPackageIndex = selectedItemPopupIndex;
                UpdateVersionText();
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            isDevelopment = GUILayout.Toggle(isDevelopment, DEVELOPMENT_LABEL);

            if (isDevelopment)
            {
                GUI.enabled = false;
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUIUtility.labelWidth = 20;
                GUILayout.Label(VERSION_TITLE_LABEL);
                versionText = EditorGUILayout.TextField(versionText, EditorStyles.textField);

                SetColorForAction(() =>
                {
                    GUILayout.Label(errorMessage);
                }, Color.red);

                SetColorForAction(() =>
                {
                    GUILayout.Label(successMessage);
                }, Color.green);

                SetColorForAction(() =>
                {
                    GUILayout.Label(isExporting ? "Exporting ..." : string.Empty);
                }, Color.white);

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(RELEASE_NOTES_TITLE_LABEL, resolutionNotesTitleStyle);

            EditorGUILayout.Space(SMALL_SPACE);

            WrapActionInAlwaysEnabled(() => noteScrollPosition = EditorGUILayout.BeginScrollView(noteScrollPosition));
            {

                for (int i = 0; i < releaseNotes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("*");

                        releaseNotes[i] = EditorGUILayout.TextArea(releaseNotes[i], textAreaStyle, GUILayout.ExpandWidth(true), GUILayout.MinWidth(position.width - 100));

                        GUILayout.Label(string.Empty);


                        if (GUILayout.Button("-", removeNoteButtonStyle))
                        {
                            GUI.FocusControl(string.Empty);
                            releaseNotes.RemoveAt(i);
                        }
                        EditorGUILayout.Space(MEDIUM_SPACE);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(TINY_SPACE);
                }
            }
            WrapActionInAlwaysEnabled(() => EditorGUILayout.EndScrollView());

            if (GUILayout.Button("+"))
            {
                releaseNotes.Add(string.Empty);
            }

            GUI.enabled = isExporting ? false : true;

            if (GUILayout.Button("EXPORT"))
            {
                errorMessage = string.Empty;
                successMessage = string.Empty;
                isExporting = true;

                DateTime _timeBeforeExport = DateTime.Now.AddSeconds(-1);
                
                if (ExportUnityPackage())
                {  
                    if (!isDevelopment)
                    {
                        PackageHTMLExporter.UpdateReleaseNotes(availablePackages[selectedItemPopupIndex], versionText, releaseNotes);
                    }

                    Task.Run(async () => await CheckIfPackageExported(_timeBeforeExport).ContinueWith((task) => {
                        UpdatePackageVersion(availablePackages[selectedItemPopupIndex]);
                        UpdateVersionText();
                        isExporting = false;
                    }));

                    releaseNotes.Clear();                
                }
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndVertical();
    }

    private void UpdateVersionText()
    {
        versionText = packages[availablePackages[selectedItemPopupIndex]].version.ToString();
    }

    private string GetCurrentPackageExportPath()
    {
        string _package = availablePackages[selectedItemPopupIndex];
        string _exportedPackageFilename;

        if (isDevelopment)
        {
            _exportedPackageFilename = $"../../releases/{_package}/{_package}-development.unitypackage";
        }
        else
        {
            _exportedPackageFilename = $"../../releases/{_package}/{_package}-{versionText}.unitypackage";
        }

        return _exportedPackageFilename;
    }

    private async Task CheckIfPackageExported(DateTime _timeBeforeExport)
    {
        string _exportedPackageFilename = GetCurrentPackageExportPath();
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken _cancellationToken = _cancellationTokenSource.Token;

        Task _checkFile = Task.Run(async () =>
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (File.Exists(_exportedPackageFilename) && File.GetLastWriteTime(_exportedPackageFilename) > _timeBeforeExport)
                {                  
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(0.5f));
            }
        }, _cancellationToken);

        TimeSpan _timeoutSpan = TimeSpan.FromSeconds(TIMEOUT_SECONDS_CHECK_IF_PACKAGE_EXPORTED);
        Task _timeoutTask = Task.Delay(_timeoutSpan, _cancellationToken);
        
        if (await Task.WhenAny(_checkFile, _timeoutTask) != _checkFile)
        {
            errorMessage += "Package export timed out";
            Debug.LogWarning($"Checking for package being exported timed out after {_timeoutSpan:%m} minutes and {_timeoutSpan:%s} seconds");
        }
        else
        {
            successMessage = "Package exported successfully";
            Debug.Log("Package Exported.");
        }

        _cancellationTokenSource.Cancel();
    }

    private static void WrapActionInAlwaysEnabled(Action _action)
    {
        bool _isGUIEnabled = GUI.enabled;

        GUI.enabled = true;
        _action.Invoke();
        GUI.enabled = _isGUIEnabled;
    }

    private static void SetColorForAction(Action _action, Color _color)
    {
        Color _colorBeforeChange = GUI.color;

        GUI.color = _color;
        _action.Invoke();
        GUI.color = _colorBeforeChange;
    }

    private void InitializeStyles()
    {
        // Initializing resolution notes title style
        resolutionNotesTitleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };

        // Initializing text area style
        textAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            stretchWidth = true,
        };

        // Initializing remove note button style
        removeNoteButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            stretchWidth = false,
            fixedWidth = 20,
            fixedHeight = 20
        };

        // Initializing export button style
        exportButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.LowerCenter
        };

        styleInitialized = true;
    }

    [DidReloadScripts]
    private static void UpdateAvailablePackages()
    {
        availablePackages = new List<string>();

        foreach(KeyValuePair<string, PackageData> _pair in packages)
        {
            string _relativePackagePath = _pair.Value.paths[0];
            string _fullPackagePath = Application.dataPath + _relativePackagePath;
            string _packageToAdd = _pair.Key;
            if (Directory.Exists(_fullPackagePath))
            {
                // Extra check as Spine and Anima2D shares the same package name
                if (_relativePackagePath.ToLower().Contains("tutotoonsintro"))
                {
                    _packageToAdd = "TutoToonsIntroSpine";
                    if (Directory.Exists(_fullPackagePath + "/Anima2D"))
                    {
                        _packageToAdd = "TutoToonsIntroAnima2D";
                    }
                    if (File.Exists(_fullPackagePath + "/.4.0.54.txt"))
                    {
                        _packageToAdd = "TutoToonsIntroSpineV4";
                    }
                }

                availablePackages.Add(_packageToAdd);
                UpdatePackageVersion(_packageToAdd);
            }                    
        }

        versionText = packages[availablePackages[selectedItemPopupIndex]].version.ToString();
    }

    private static void UpdatePackageVersion(string _packageName)
    {
        string _packageReleasePath = $"../../releases/{_packageName}";
        Version _latestVersion = new Version(1, 0, 0);

        if (Directory.Exists(_packageReleasePath)) 
        {
            string[] _files = Directory.GetFiles(_packageReleasePath);
            FindLatestVersionInPackage(ref _latestVersion, _files);
        }
        
        packages[_packageName].version = new Version(_latestVersion.Major, _latestVersion.Minor, _latestVersion.Build + 1);
    }

    private static void FindLatestVersionInPackage(ref Version _latest, string[] _files)
    {
        foreach (string _file in _files)
        {
            string _fileName = Path.GetFileNameWithoutExtension(_file);
            string _fileExtension = Path.GetExtension(_file);
            if ( _fileExtension != ".unitypackage" || _fileName.Contains("development"))
            {
                continue;
            }

            string _versionText = _fileName.Substring(_fileName.LastIndexOf('-') + 1);

            if(string.IsNullOrEmpty(_versionText))
            {
                continue;
            }

            Version _version = Version.Parse(_versionText);
            if (_version > _latest)
            {
                _latest = _version;
            }
        }
    }

    private bool ExportUnityPackage()
    {
        if (availablePackages.Count <= 0 || (!isDevelopment && string.IsNullOrEmpty(versionText)))
        {
            return false;
        }

        string _package = availablePackages[selectedItemPopupIndex];

        string _exportedPackageFilename = GetCurrentPackageExportPath();

        if (!isDevelopment)
        {            
            bool _exportFailed = false;

            if(releaseNotes.Count == 0 || string.IsNullOrEmpty(releaseNotes[0]))
            {
                errorMessage += "- Release must contain at least one non empty release note\n";
                _exportFailed = true;
            }     

            if (File.Exists(_exportedPackageFilename))
            {
                errorMessage += "- Package with specified version already exists\n";
                _exportFailed = true;
            }

            if (_exportFailed)
            {
                isExporting = false;
                return false;
            }

        }

        List<string> _assetsToExport = new List<string>();

        foreach (string _pathToAdd in packages[_package].paths)
        {
            AddFilesToList(Application.dataPath + _pathToAdd, _assetsToExport);
        }

        UpdateCurrentPackageVersionInScript();
        AssetDatabase.ExportPackage(_assetsToExport.ToArray(), _exportedPackageFilename, ExportPackageOptions.Default | ExportPackageOptions.Interactive);

        return true;
    }

    private void UpdateCurrentPackageVersionInScript()
    {
        string _package = availablePackages[selectedItemPopupIndex];
        string _packageScriptName = $"{_package.Replace("Package", string.Empty)}PackageVersions.cs";
        string _packageFolderName = packages[_package].paths[0];
        string _packageVersion = isDevelopment ? "development" : versionText;
        List<PackageVersion> _packageVersions = new List<PackageVersion>()
        {
            new PackageVersion(_package, _packageVersion, DateTime.Now),
        };

        PackageVersionsUtils.SetPackageVersionsInScript(_packageVersions, _packageScriptName, _packageFolderName);
    }

    private static void AddFilesToList(string _path, List<string> _list)
    {
        foreach (string _dirPath in Directory.GetDirectories(_path))
        {
            AddFilesToList(_dirPath, _list);
        }

        foreach (string _filePath in Directory.GetFiles(_path))
        {
            if (_filePath.EndsWith(".meta"))
            {
                continue;
            }
            int _pos = Application.dataPath.Length;
            _list.Add("Assets" + _filePath.Substring(_pos));
        }
    }
}