using System;
using System.Globalization;

public class PackageVersion
{
    public string name;
    public string version;
    public DateTime? date;

    public PackageVersion(string _name, string _version, DateTime? _date = null)
    {
        this.name = _name;
        this.version = _version;
        this.date = _date;
    }

    public override string ToString()
    {   
        return $"{name}: {version} {GetFormattedDate()}";
    }

    public string GetFormattedName()
    {
        string _formattedPackageName = string.Empty;
        string _textToRemoveFromName = "Package";

        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && (i + 1 < name.Length && char.IsLower(name[i + 1])))
            {
                _formattedPackageName += $" {name[i]}";
            }
            else
            {
                _formattedPackageName += name[i];
            }
        }

        _formattedPackageName = _formattedPackageName.Replace(_textToRemoveFromName, string.Empty);
        _formattedPackageName = $"{_formattedPackageName} Package";
        _formattedPackageName = _formattedPackageName.Trim(' ');
        return _formattedPackageName;
    }

    private string GetFormattedDate()
    {
        if(!date.HasValue)
        {
            return string.Empty;
        }

        return string.Concat(date.Value.ToString("MMMM", CultureInfo.InvariantCulture), " ", date.Value.Day.ToString("00"), ", ", date.Value.Year);
    }
}