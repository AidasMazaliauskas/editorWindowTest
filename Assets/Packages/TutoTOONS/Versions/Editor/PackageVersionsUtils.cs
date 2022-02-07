using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class PackageVersionsUtils
{
    private const string PACKAGE_GENERATED_VERSION_INSERT_TAG = "// Generated package versions";
    private const string PACKAGE_MANUAL_VERSION_INSERT_TAG = "// Manually inserted package versions";

    public static void SetPackageVersionsInScript(List<PackageVersion> _packageVersions, string _packageScriptName, string _packageFolderName)
    {
        if(_packageVersions == null || _packageVersions.Count == 0 || string.IsNullOrEmpty(_packageScriptName) || string.IsNullOrEmpty(_packageFolderName))
        {
            return;
        }

        string _scriptPath = $"{Application.dataPath}{_packageFolderName}/Versions/{_packageScriptName}";

        if(!File.Exists(_scriptPath))
        {
            return;
        }

        string[] _scriptLines = File.ReadAllLines(_scriptPath);
        int _packageVersionInsertTagIndex = IndexOfValueInScript(_scriptLines, PACKAGE_GENERATED_VERSION_INSERT_TAG);

        if(_packageVersionInsertTagIndex < 0)
        {
            Debug.LogError($"Missing package version insert tag in script: {_scriptPath}\nAdd '{PACKAGE_GENERATED_VERSION_INSERT_TAG}' tag in List initialization");
            return;
        }

        int _amountOfInsertLinePadding = _scriptLines[_packageVersionInsertTagIndex].TakeWhile(c => char.IsWhiteSpace(c)).Count();

        _scriptLines = RemoveOldGeneratedVersionsInScript(_scriptLines, _packageVersionInsertTagIndex + 1);

        foreach (PackageVersion _version in _packageVersions)
        {
            _scriptLines[_packageVersionInsertTagIndex] += $"\n{new string(' ', _amountOfInsertLinePadding)}new {nameof(PackageVersion)}(\"{_version.GetFormattedName()}\", \"{_version.version}\"{GetDateTimeParameter(_version.date)}),";
        }

        _scriptLines[_packageVersionInsertTagIndex] += "\n";

        File.WriteAllLines(_scriptPath, _scriptLines);
    }

    private static string[] RemoveOldGeneratedVersionsInScript(string[] _scriptLines, int _startIndex)
    {
        if(_startIndex < 0 || _startIndex > _scriptLines.Length || _scriptLines == null || _scriptLines.Length == 0)
        {
            return _scriptLines;
        }

        int[] _lineIndexesToRemove = _scriptLines.Skip(_startIndex)
            .TakeWhile(line => !line.Contains("};") && !line.Contains(PACKAGE_MANUAL_VERSION_INSERT_TAG))
            .Select((line, idx) => idx + _startIndex)
            .ToArray();

        return _scriptLines.Where((_, idx) => !_lineIndexesToRemove.Contains(idx)).ToArray();
    }

    private static int IndexOfValueInScript(string[] _scriptLines, string _value, int _startIndex = 0)
    {
        Regex _stringValueRegex = new Regex(_value);

        _startIndex = Mathf.Clamp(_startIndex, 0, _scriptLines.Length);

        for (int i = _startIndex; i < _scriptLines.Length; i++)
        {
            if (_stringValueRegex.IsMatch(_scriptLines[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private static string GetDateTimeParameter(DateTime? _date)
    {
        if (_date.HasValue)
        {
            return $", new System.DateTime({_date.Value.Year}, {_date.Value.Month}, {_date.Value.Day})";
        }

        return string.Empty;
    }
}
