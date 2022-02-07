using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class PackageHTMLExporter
{
    private const string PROP_ID_NOTES_TITLE = "id=\"generator-package-release-notes-title\"";
    private const string PROP_ID_CONTAINER =    "id=\"generator-package-release-container\"";
    private const string PROP_ID_DATE =         "id=\"generator-package-release-date\"";
    private const string PROP_ID_VERSION =      "id=\"generator-package-release-version\"";
    private const string PROP_ID_LIST =         "id=\"generator-package-release-list\"";
    private const string PROP_ID_LIST_ITEM =    "id=\"generator-package-release-list-item\"";

    public static void UpdateReleaseNotes(string _packageName, string _packageVersion, IEnumerable<string> _releaseNotes)
    {
        string _releaseNotesPath = $"../../releases/{_packageName}/_release_notes.htm";

        if (!File.Exists(_releaseNotesPath))
        {
            Debug.LogError($"Notes weren't added to Release Notes. Release Notes file not found at: {Path.GetFullPath(_releaseNotesPath)}");
            return;
        }

        try
        {
            int _continueFrom = 0;
            string[] _lines = File.ReadAllLines(_releaseNotesPath);

            _continueFrom = SetReleaseNoteTitle(_packageName, _lines, _continueFrom);
            _lines = InsertNotesContainer(_continueFrom, _lines, _packageVersion, _releaseNotes);
            UpdateFile(_releaseNotesPath, _lines);
        }
        catch (Exception _ex)
        {
            Debug.LogError($"Notes weren't added to Release Notes. {_ex.Message}");
        }
    }

    private static string GetExportDataReleaseContainer(string _packageVersion, IEnumerable<string> _releaseNotes)
    {      
        return $"\t\t<div {PROP_ID_CONTAINER}>\n" +
               $"\t\t\t<h2 {PROP_ID_DATE}> {GetCurrentDateAsString()} </h2>\n" +
               $"\t\t\t<h3 {PROP_ID_VERSION}> Package Version {_packageVersion} </h3>\n" +
                "\n" +
               $"\t\t\t<ul {PROP_ID_LIST}>\n" +
               NoteListItems(_releaseNotes, 4) +                 
               $"\t\t\t</ul>\n" +                            
               $"\t\t</div>\n";   
    }

    private static void UpdateFile(string _path, string[] _lines)
    {
        File.WriteAllLines(_path, _lines);
    }

    private static int SetReleaseNoteTitle(string _packageName, string[] _lines, int _continueFrom)
    {
        for (int i = _continueFrom; i < _lines.Length; i++)
        {
            if (CheckIfTitle(_lines[i]))
            {
                string _formattedPackageName = GetFormattedPackageName(_packageName);
                string _titleTagPattern = "<h1.*>";
                string _replacement = $"<h1 {PROP_ID_NOTES_TITLE}>{_formattedPackageName} Package Release Notes</h1>";
                string _newTitleLine = Regex.Replace(_lines[i], _titleTagPattern, _replacement);

                _lines[i] = _newTitleLine;

                return i + 1;
            }
        }

        Debug.LogError($"Release Notes title not found in file. Header should contain '{PROP_ID_NOTES_TITLE}' property");
        return -1;
    }

    private static string GetFormattedPackageName(string _packageName)
    {
        string _formattedPackageName = string.Empty;
        string _textToRemoveFromName = "Package";

        for(int i = 0; i < _packageName.Length; i++)
        {
            if(char.IsUpper(_packageName[i]) && (i + 1 < _packageName.Length && char.IsLower(_packageName[i + 1])))
            {
                _formattedPackageName += $" {_packageName[i]}";
            }
            else
            {
                _formattedPackageName += _packageName[i];
            }
        }

        _formattedPackageName = _formattedPackageName.Replace(_textToRemoveFromName, string.Empty);
        _formattedPackageName = _formattedPackageName.Trim(' ');
        return _formattedPackageName;
    }

    private static string[] InsertNotesContainer(int _insertIndex, string[] _lines, string _packageVersion, IEnumerable<string> _releaseNotes)
    {
        string _container = GetExportDataReleaseContainer(_packageVersion, _releaseNotes);
        string[] _containerLines = _container.Split('\n');

        int _countOfNewFileLines = _lines.Length + _containerLines.Length;
        string[] _newLines = new string[_countOfNewFileLines];

        int _leftToInsert = _containerLines.Length;
        for(int i = 0; i < _countOfNewFileLines; i++)
        {
            if(i < _insertIndex)
            {
                _newLines[i] = _lines[i];
                continue;
            }

            if(_leftToInsert > 0)
            {
                _newLines[i] = _containerLines[_containerLines.Length - _leftToInsert];
                _leftToInsert--;
            }
            else
            {
                _newLines[i] = _lines[i - _containerLines.Length];
            }
        }

        return _newLines;
    }

    private static bool CheckIfTitle(string _line)
    {
        return _line.Contains(PROP_ID_NOTES_TITLE);
    }

    private static string GetCurrentDateAsString()
    {
        DateTime _d = DateTime.Now;
        return string.Concat(_d.ToString("MMMM", CultureInfo.InvariantCulture), " ", _d.Day.ToString("00"), ", ", _d.Year);
    }

    private static string NoteListItems(IEnumerable<string> _releaseNotes, int _indent)
    {
        string _formattedNotes = string.Empty;

        foreach (string _note in _releaseNotes)
        {
            _formattedNotes += $"{Tab(_indent)}<li {PROP_ID_LIST_ITEM}>{_note}</li>\n";
        }

        return _formattedNotes;
    }

    private static string Tab(int _indent)
    {
        return new string('\t', _indent);
    }
}
