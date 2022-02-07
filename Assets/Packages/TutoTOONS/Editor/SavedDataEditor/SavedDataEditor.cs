using System.Collections.Generic;
using System.IO;
using TutoTOONS;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class SavedDataEditor : EditorWindow
{
    #region Constants
    private const float RESIZE_CONTROL_RECT_WIDTH = 10;
    private const float RESIZE_COLUMN_MIN_LEFT = 100;
    private const float RESIZE_COLUMN_MIN_RIGHT = 200;
    private const int TABLE_CONTENT_PADDING_TOP = 5;
    private const int TABLE_CONTENT_PADDING_BOTTOM = 20;
    private const int TABLE_SCROLL_SPEED = 5;
    private const int SEARCH_BAR_MIN_WIDTH = 280;
    private const int INITIAL_KEY_COLUMN_WIDTH = 210;
    #endregion

    private static bool styleInitialized = false;
    private static readonly GUIContent exportButton = new GUIContent("Export", "Click to export entered key value pairs as JSON.");
    private static readonly GUIContent importButton = new GUIContent("Import", "Click to import Saved Data JSON.");
    private static readonly GUIContent saveButton = new GUIContent("Save", "Click to apply changes to \"saved_data.json\"");
    private static readonly GUIContent clearButton = new GUIContent("Clear", "Click to clear \"saved_data.json\"");
    private static GUIContent refreshButton;
    private static string searchText = string.Empty;
    private static string errorMessage = string.Empty;
    private static string successMessage = string.Empty;
    private bool resizeTableHeader;
    private float headerWidth;
    private float keyContainerWidth;
    private float keyTextFieldWidth;
    private float valueContainerWidth;
    private float valueTextFieldWidth;
    private float keyScrollbarValue;
    private float valueScrollbarValue;
    private Vector2 keyScrollPosition;
    private Vector2 valueScrollPosition;
    private Vector2 tableContentScrollPosition;
    private Rect searchBarRect;
    private SavedDataParserPairs pairs;
    private List<bool> isPairFiltered;

    #region GUI Styles
    private static GUIStyle windowContainerStyle;
    private static GUIStyle mainContainerStyle;
    private static GUIStyle tableContentContainerStyle;
    private static GUIStyle tableHeaderContainerStyle;
    private static GUIStyle buttonContainerStyle;
    private static GUIStyle headerTextStyle;
    private static GUIStyle inputFieldStyle;
    private static GUIStyle removeButtonStyle;
    private static GUIStyle removeButtonContainerStyle;
    private static GUIStyle addButtonStyle;
    private static GUIStyle searchBarStyle;
    private static GUIStyle errorMessageStyle;
    private static GUIStyle successMessageStyle;
    #endregion

    [MenuItem("TutoTOONS/Saved Data/Saved Data Editor")]
    public static void Open()
    {
        SavedDataEditor _window = EditorWindow.GetWindow<SavedDataEditor>("Saved Data Editor");
        _window.Show();
        _window.minSize = new Vector2(300, 180);      
    }

    [MenuItem("TutoTOONS/Saved Data/Clear Saved Data")]
    public static void ClearSavedData()
    {
        if (EditorUtility.DisplayDialog("Saved data will be removed", "Are you sure you want to clear saved data?", "Yes", "No"))
        {
            TutoTOONS.SavedData.Clear();
            PlayerPrefs.DeleteAll();
            Debug.Log("All saved data was cleared.");
        }
    }

    private void Awake()
    {
        LoadSavedData();        
    }

    private void OnEnable()
    {       
        keyScrollbarValue = 0f;
        ClearMessages();
        headerWidth = INITIAL_KEY_COLUMN_WIDTH;
        refreshButton = new GUIContent(EditorGUIUtility.IconContent("d_RotateTool@2x"))
        {
            tooltip = "Update table with values from \"saved_data.json\""
        };
    }

    private void OnGUI()
    {
        float _clamppedHeaderWidth = Mathf.Clamp(headerWidth - RESIZE_CONTROL_RECT_WIDTH + 1
                            , RESIZE_COLUMN_MIN_LEFT, position.width - RESIZE_COLUMN_MIN_RIGHT);

        bool _isRepaint = Event.current.type == EventType.Repaint;

        if (!styleInitialized)
        {
            InitializeStyles();
        }

        SearchBar();
        FilterPairs();

        GUILayout.BeginVertical(windowContainerStyle);
        {
            GUILayout.BeginVertical(mainContainerStyle);
            {               
                if (pairs.pairs.Count > 0)
                {
                    TableHeader(_clamppedHeaderWidth);
                    bool _tableHasContent = TableContent(_clamppedHeaderWidth, _isRepaint);
                    if (_tableHasContent)
                    {
                        TableFooter();
                    }

                    ButtonAdd();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Saved Data is empty. Please add data by clicking add (+) or import the saved data file.", EditorStyles.wordWrappedLabel);
                        LoadSavedDataButton();
                        GUILayout.Space(25);
                    }
                    GUILayout.EndHorizontal();

                    ButtonAdd();
                }

                
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(buttonContainerStyle);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(exportButton))
                    {
                        ClearMessages();
                        ExportSavedData();
                    }

                    if (GUILayout.Button(importButton))
                    {
                        ClearMessages();
                        if (ImportSavedData())
                        {
                            SavedData.Clear();
                            SaveData();
                        }
                    }

                    if (GUILayout.Button(clearButton))
                    {
                        ClearSavedData();
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(saveButton))
                    {
                        ClearMessages();
                        SaveData();
                    }
                }
                GUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    GUILayout.Label(errorMessage, errorMessageStyle);
                }

                if (!string.IsNullOrEmpty(successMessage))
                {
                    GUILayout.Label(successMessage, successMessageStyle);
                }
            }
            GUILayout.EndVertical();     
        }
        GUILayout.EndVertical();
    }

    private void ClearMessages()
    {
        successMessage = string.Empty;
        errorMessage = string.Empty;
    }

    private void SaveData()
    {
        if (ArePairsValid())
        {
            bool _savedSuccessfully = SavedData.Save(GetPairsDictionary());
            if (_savedSuccessfully)
            {
                successMessage += "Saved data updated successfully.\n";
            }
            else
            {
                errorMessage += "Failed saving data.";
            }
        }   
    }

    private void LoadSavedData()
    {
        pairs = SavedData.ReadSavedData(Path.Combine(Application.persistentDataPath, SavedData.SAVED_DATA_FILE));

        if (pairs == null)
        {
            InitPairs();
        }
        else
        {
            isPairFiltered = new List<bool>();
        }

        foreach (SavedDataParserPair _ in pairs.pairs)
        {
            isPairFiltered.Add(true);
        }
    }

    private IDictionary<string, string> GetPairsDictionary()
    {
        IDictionary<string, string> _pairDictionary = new Dictionary<string, string>();
        foreach (SavedDataParserPair _pair in pairs.pairs)
        {
            _pairDictionary.Add(_pair.k, _pair.v);
        }

        return _pairDictionary;
    }

    private bool ImportSavedData()
    {
        string _filePath = EditorUtility.OpenFilePanel("Import Save Data", Application.persistentDataPath, "json");

        if (string.IsNullOrEmpty(_filePath))
        {
            return false;
        }

        pairs = SavedData.ReadSavedData(_filePath);

        if (pairs == null || pairs.pairs.Count == 0)
        {
            errorMessage += "Failed to import saved data or it's empty. Make sure the JSON file valid.\n";
            InitPairs();
            return false;
        }

        foreach (SavedDataParserPair _ in pairs.pairs)
        {
            isPairFiltered.Add(true);
        }

        successMessage += "Saved data imported successfully.\n";

        return true;
    }

    private void InitPairs()
    {
        pairs = new SavedDataParserPairs
        {
            pairs = new List<SavedDataParserPair>()
        };
        isPairFiltered = new List<bool>();
    }

    private void ExportSavedData()
    {
        if (!ArePairsValid())
        {         
            return;
        }

        string _filePath = EditorUtility.SaveFilePanel("Export Save Data", Application.persistentDataPath, "saved_data", "json");

        if (!string.IsNullOrEmpty(_filePath))
        {
            SavedData.Export(GetPairsDictionary(), _filePath);
            EditorUtility.RevealInFinder(_filePath);
            successMessage += "Saved data exported successfully.\n";
        }
    }

    private void FilterPairs()
    {
        string _formattedSearchText = searchText.ToLower();

        for (int i = 0; i < pairs.pairs.Count; i++)
        {
            isPairFiltered[i] = string.IsNullOrEmpty(_formattedSearchText) || 
                pairs.pairs[i].k.ToLower().Contains(_formattedSearchText) || 
                pairs.pairs[i].v.ToLower().Contains(_formattedSearchText);
        }
    }

    private bool ArePairsValid()
    {
        bool _areValid = true;
        errorMessage = string.Empty;

        if(!pairs.pairs.TrueForAll(x => !string.IsNullOrEmpty(x.k)))
        {
            _areValid = false;
            errorMessage += "- Saved data cannot contain empty key values\n";
        }

        IEnumerable<string> _duplicateKeys = pairs.pairs.GroupBy(x => x.k)
            .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key))
            .Select(y => y.Key);

        if (_duplicateKeys.Any())
        {
            _areValid = false;
            errorMessage += "- Keys must be unique: ";
            foreach (string _key in _duplicateKeys)
            {
                errorMessage += $"'{_key}' ";
            }

            errorMessage += "\n";
        }

        return _areValid;
    }

    private void ButtonAdd()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", addButtonStyle))
            {
                SavedDataParserPair newPairEntry = new SavedDataParserPair()
                {
                    k = string.Empty,
                    v = string.Empty
                };
                pairs.pairs.Add(newPairEntry);
                isPairFiltered.Add(true);
            }
        }
        GUILayout.EndHorizontal();
    }

    private void SearchBar()
    {
        GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        {
            GUILayout.FlexibleSpace();
            bool _clickedMouse = Event.current.rawType == EventType.MouseDown; // Getting mouse position before TextField as it consumes the MouseDown event
            searchText = GUILayout.TextField(searchText, searchBarStyle, GUILayout.MinWidth(SEARCH_BAR_MIN_WIDTH), GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                searchBarRect = GUILayoutUtility.GetLastRect();
            }

            Rect _searchcClearButtonRect = GetSearchClearButtonRect();
            bool _clickedInsideButton = _searchcClearButtonRect.Contains(Event.current.mousePosition) && _clickedMouse;

            if (!string.IsNullOrEmpty(searchText) && (GUI.Button(_searchcClearButtonRect, string.Empty, GUI.skin.FindStyle("ToolbarSeachCancelButton")) || _clickedInsideButton))
            {
                searchText = string.Empty;
                GUI.FocusControl(null);
            }
        }
        GUILayout.EndHorizontal();   
    }

    private Rect GetSearchClearButtonRect()
    {
        Rect _clearSearchButtonRect = searchBarRect;
        _clearSearchButtonRect.position = new Vector2(searchBarRect.position.x + searchBarRect.width - 16, searchBarRect.position.y);
        _clearSearchButtonRect.width = 15;
        _clearSearchButtonRect.height = 15;
        return _clearSearchButtonRect;
    }

    private void TableHeader(float _clamppedHeaderWidth)
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginHorizontal(tableHeaderContainerStyle);
            {
                GUILayout.Label("Key", headerTextStyle, GUILayout.Width(_clamppedHeaderWidth - RESIZE_CONTROL_RECT_WIDTH + 1));
                TableResizeControl();
                GUILayout.Label("Value", headerTextStyle);
                LoadSavedDataButton();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(23);
        }
        GUILayout.EndHorizontal();
    }

    private void LoadSavedDataButton()
    {
        if (GUILayout.Button(refreshButton, GUILayout.Width(25), GUILayout.Height(25)))
        {
            LoadSavedData();
        }
    }

    private bool TableContent(float _clamppedHeaderWidth, bool _isRepaint)
    {
        bool _tableContainsContent = isPairFiltered.Any(x => x == true);

        tableContentScrollPosition = GUILayout.BeginScrollView(tableContentScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
        {
            ControlMainScrollPosition();

            GUILayout.BeginHorizontal();
            {
                if (_tableContainsContent)
                {
                    TableKeyColumn(_clamppedHeaderWidth, _isRepaint);
                    TableValueColumn(_isRepaint);
                    TableRemoveButtonColumn();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Nothing found.");
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        return _tableContainsContent;
    }

    private void TableFooter()
    {
        GUILayout.BeginHorizontal();
        {
            ScrollbarKeyColumn();
            ScrollbarValueColumn();
        }
        GUILayout.EndHorizontal();
    }

    private void ScrollbarKeyColumn()
    {
        if (keyContainerWidth >= keyTextFieldWidth + 6)
        {
            GUILayout.Space(keyContainerWidth);
            return;
        }

        float size = (keyContainerWidth / (keyTextFieldWidth + 6)) * keyContainerWidth;
        keyScrollbarValue = GUILayout.HorizontalScrollbar(keyScrollbarValue, size, 0, (keyTextFieldWidth + 6), GUI.skin.horizontalScrollbar, GUILayout.Width(keyContainerWidth));
        keyScrollPosition.x = keyScrollbarValue;
    }

    private void ScrollbarValueColumn()
    {
        if (valueContainerWidth >= valueTextFieldWidth + 6)
        {
            GUILayout.Space(valueContainerWidth);
            return;
        }

        float size = (valueContainerWidth / (valueTextFieldWidth + 6)) * valueContainerWidth;
        valueScrollbarValue = GUILayout.HorizontalScrollbar(valueScrollbarValue, size, 0, (valueTextFieldWidth + 6), GUI.skin.horizontalScrollbar, GUILayout.Width(valueContainerWidth));
        valueScrollPosition.x = valueScrollbarValue;
    }

    private void TableKeyColumn(float _clamppedHeaderWidth, bool _isRepaint)
    {
        GUILayout.BeginVertical(tableContentContainerStyle, GUILayout.Width(_clamppedHeaderWidth));
        {
            GUILayout.BeginScrollView(keyScrollPosition, GUIStyle.none, GUIStyle.none, GUILayout.ExpandHeight(false));
            keyScrollPosition.y = 0;
            {
                GUILayout.BeginVertical();
                {
                    for (int i = 0; i < pairs.pairs.Count; i++)
                    {
                        if (isPairFiltered[i])
                        {
                            pairs.pairs[i].k = GUILayout.TextField(pairs.pairs[i].k, inputFieldStyle);
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Text);
                        }                     
                    }
                    
                    if (_isRepaint)
                    {
                        keyTextFieldWidth = GUILayoutUtility.GetLastRect().width;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            if (_isRepaint)
            {
                keyContainerWidth = GUILayoutUtility.GetLastRect().width;
            }
        }
        GUILayout.EndVertical();
    }

    private void TableValueColumn(bool _isRepaint)
    {
        GUILayout.BeginVertical(tableContentContainerStyle);
        {
            valueScrollPosition = GUILayout.BeginScrollView(valueScrollPosition, GUIStyle.none, GUIStyle.none);
            valueScrollPosition.y = 0;       
            {
                for (int i = 0; i < pairs.pairs.Count; i++)
                {
                    if (isPairFiltered[i])
                    {
                        pairs.pairs[i].v = GUILayout.TextField(pairs.pairs[i].v, inputFieldStyle);
                        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Text);
                    }
                }

                if (_isRepaint)
                {
                    valueTextFieldWidth = GUILayoutUtility.GetLastRect().width;
                }
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        if (_isRepaint)
        {
            valueContainerWidth = GUILayoutUtility.GetLastRect().width;
        }
    }

    private void TableRemoveButtonColumn()
    {
        GUILayout.BeginVertical(removeButtonContainerStyle);
        {
            for (int i = 0; i < pairs.pairs.Count; i++)
            {
                if (isPairFiltered[i])
                {
                    if (GUILayout.Button("-", removeButtonStyle))
                    {
                        GUI.FocusControl(string.Empty);
                        pairs.pairs.RemoveAt(i);
                        isPairFiltered.RemoveAt(i);
                    }
                }
            }
        }
        GUILayout.EndVertical();
    }

    private void ControlMainScrollPosition()
    {
        Event _evt = Event.current;
        if (_evt.type == EventType.ScrollWheel)
        {
            tableContentScrollPosition.y = Mathf.Clamp(tableContentScrollPosition.y + _evt.delta.y * TABLE_SCROLL_SPEED, 0, float.MaxValue);
        }
    }

    private void TableResizeControl()
    {
        int _controlId = GUIUtility.GetControlID(FocusType.Passive);
        Event _evt = Event.current;

        GUILayout.Box(string.Empty, GUILayout.Width(2f));
        Rect _resizeControlRect = GUILayoutUtility.GetLastRect();
        _resizeControlRect.width += RESIZE_CONTROL_RECT_WIDTH;
        _resizeControlRect.x -= RESIZE_CONTROL_RECT_WIDTH / 2;
        EditorGUIUtility.AddCursorRect(_resizeControlRect, MouseCursor.ResizeHorizontal);

        EventType eventType = _evt.GetTypeForControl(_controlId);

        if (eventType == EventType.MouseDown && _resizeControlRect.Contains(Event.current.mousePosition))
        {
            GUIUtility.hotControl = _controlId;
            resizeTableHeader = true;
        }

        if (resizeTableHeader)
        {
            headerWidth = _evt.mousePosition.x;
            Repaint();
        }
    
        if (eventType == EventType.MouseUp)
        {          
            resizeTableHeader = false;
        }
    }

    private void InitializeStyles()
    {
        windowContainerStyle = new GUIStyle()
        {
            padding = new RectOffset(),
            margin = new RectOffset(),
            border = new RectOffset()
        };

        mainContainerStyle = new GUIStyle()
        {
            padding = new RectOffset(10, 10, 10, 10)
        };

        tableContentContainerStyle = new GUIStyle(EditorStyles.helpBox)
        {       
            border = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, TABLE_CONTENT_PADDING_TOP, TABLE_CONTENT_PADDING_BOTTOM),
            margin = new RectOffset(0, 0, 0, 0),
        };

        tableHeaderContainerStyle = new GUIStyle(EditorStyles.helpBox)
        {
            border = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 5, 5),
            margin = new RectOffset(0, 0, 0, 0),
        };

        buttonContainerStyle = new GUIStyle(GUI.skin.FindStyle("Toolbar"))
        {
            fixedHeight = 60,
            padding = new RectOffset(10, 10, 0, 0)
        };

        headerTextStyle = new GUIStyle(EditorStyles.boldLabel);

        inputFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            fixedHeight = 20
        };

        removeButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            stretchWidth = false,
            fixedWidth = 20,
            fixedHeight = 20
        };

        removeButtonContainerStyle = new GUIStyle()
        {
            stretchWidth = false,
            padding = new RectOffset(0, 0, TABLE_CONTENT_PADDING_TOP + 2, TABLE_CONTENT_PADDING_BOTTOM)
        };

        addButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            margin = new RectOffset(0, 35, 0, 0),
            fixedWidth = 25,
            fixedHeight = 25
        };

        searchBarStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachTextField"));

        errorMessageStyle = new GUIStyle(EditorStyles.label);
        errorMessageStyle.normal.textColor = Color.red;

        successMessageStyle = new GUIStyle(EditorStyles.label);
        successMessageStyle.normal.textColor = Color.green;

        styleInitialized = true;  
    }
}