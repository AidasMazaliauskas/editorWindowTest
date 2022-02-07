using System;
using System.Globalization;
using UnityEngine;

namespace TutoTOONS
{
    public class SystemUtils
    {
        private static int MAX_DATE_DIFF_SECONDS = 604800; // 1 week in seconds
        private static CultureInfo current_culture;
        
        //This is required for games with "air." bundle ID prefix. Games must always use bundle ID without "air.".
        public static string GetBundleID(string _bundle_id = null)
        {
            if (_bundle_id == null)
            {
                _bundle_id = Application.identifier;
            }
            if (_bundle_id.IndexOf("air.") == 0)
            {
                _bundle_id = _bundle_id.Substring(4);
            }
            return _bundle_id;
        }

        public static int ConvertVersionStringToCode(string version)
        {
            int versionCode = 0;
            string[] versionSplit = version.Split('.');
            int versionInt = 0;

            try
            {
                for (int i = 0; i < versionSplit.Length; i++)
                {
                    if (int.TryParse(versionSplit[i], out versionInt))
                    {
                        versionCode = checked(versionCode * 1000 + versionInt);
                    }
                    else
                    {
                        Debug.LogError("SystemUtils.ConvertVersionStringToCode(string version) - string parameter couldn't be converted into an int");
                        return -1;
                    }
                }
            }
            catch (System.OverflowException e)
            {
                Debug.LogError("SystemUtils.ConvertVersionStringToCode(string version) - CHECKED and CAUGHT: " + e.ToString());
                return -1;
            }

            return versionCode;
        }

        public static string ConvertVersionCodeToString(int code)
        {
            //return string.Format((code.))
            return string.Format((code / 1000000) + "_" + (code / 1000 % 1000) + "_" + (code % 1000));
        }

        public static DateTime DateTimeParse(string _date_string, DateTime _default_date_time)
        {
            DateTime _parsed_date = DateTime.MinValue;

            // Initial date time parsing
            try
            {
                _parsed_date = DateTime.Parse(_date_string);
                return GetValidatedDate(_parsed_date, _default_date_time);
            }
            catch (Exception e)
            {
                Debug.Log("DateTime parse failed with error: " + e.Message);
            }

            // Try parse with latest working culture
            try
            {
                if (current_culture != null)
                {
                    _parsed_date = DateTime.Parse(_date_string, current_culture);
                    return GetValidatedDate(_parsed_date, _default_date_time);
                }
            }
            catch (Exception e)
            {
                Debug.Log("DateTime parse failed with error: " + e.Message);
            }

            // Try parse with all possible cultures
            foreach (CultureInfo _culture_info in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                try
                {
                    _parsed_date = DateTime.Parse(_date_string, _culture_info);
                    current_culture = _culture_info;
                    return GetValidatedDate(_parsed_date, _default_date_time);
                }
                catch (Exception e)
                {
                    Debug.Log("DateTime parse failed with error: " + e.Message);
                }
            }

            return _default_date_time;
        }

        private static DateTime GetValidatedDate(DateTime _date_to_validate, DateTime _default_date_time)
        {
            // Check if date is within current and max time
            double _date_diff = (_date_to_validate - DateTime.Now).TotalSeconds;

            if(_date_diff < MAX_DATE_DIFF_SECONDS)
            {
                return _date_to_validate;
            }

            // Try to retrieve lowest possible date
            try
            {
                DateTime _new_date = DateTime.MinValue;
                DateTime _alternative_date = new DateTime(_date_to_validate.Year, _date_to_validate.Day, _date_to_validate.Month, _date_to_validate.Hour, _date_to_validate.Minute, _date_to_validate.Second, 000);

                if(_date_to_validate < _alternative_date)
                {
                    _new_date = _date_to_validate;
                }
                else
                {
                    _new_date = _alternative_date;
                }

                double _new_date_diff = (_new_date - DateTime.Now).TotalSeconds;

                if (_new_date_diff < MAX_DATE_DIFF_SECONDS)
                {
                    return _new_date;
                }
                else
                {
                    return _default_date_time;
                }
            }
            catch(Exception e)
            {
                Debug.Log("GetValidatedDate failed to retrieve alternative date with error: " + e.Message);
            }

            return _default_date_time;
        }
        
        public static DateTime DateTimeParse(string _date_string)
        {
            return DateTimeParse(_date_string, DateTime.Now);
        }

        public static string GetSystemLanguageISOCode()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Afrikaans: 
                    return "af";
                case SystemLanguage.Arabic: 
                    return "ar";
                case SystemLanguage.Basque: 
                    return "eu";
                case SystemLanguage.Belarusian: 
                    return "be";
                case SystemLanguage.Bulgarian: 
                    return "bg";
                case SystemLanguage.Catalan: 
                    return "ca";
                case SystemLanguage.Chinese: 
                    return "zh";
                case SystemLanguage.Czech: 
                    return "cs";
                case SystemLanguage.Danish: 
                    return "da";
                case SystemLanguage.Dutch: 
                    return "nl";
                case SystemLanguage.English: 
                    return "en";
                case SystemLanguage.Estonian: 
                    return "et";
                case SystemLanguage.Faroese: 
                    return "fo";
                case SystemLanguage.Finnish: 
                    return "fi";
                case SystemLanguage.French: 
                    return "fr";
                case SystemLanguage.German: 
                    return "de";
                case SystemLanguage.Greek: 
                    return "el";
                case SystemLanguage.Hebrew: 
                    return "he";
                case SystemLanguage.Hungarian: 
                    return "hu";
                case SystemLanguage.Icelandic: 
                    return "is";
                case SystemLanguage.Indonesian: 
                    return "id";
                case SystemLanguage.Italian: 
                    return "it";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Korean: 
                    return "ko";
                case SystemLanguage.Latvian: 
                    return "lv";
                case SystemLanguage.Lithuanian: 
                    return "lt";
                case SystemLanguage.Norwegian: 
                    return "no";
                case SystemLanguage.Polish: 
                    return "pl";
                case SystemLanguage.Portuguese: 
                    return "pt";
                case SystemLanguage.Romanian: 
                    return "ro";
                case SystemLanguage.Russian:
                    return "ru";
                case SystemLanguage.SerboCroatian: 
                    return "sr";
                case SystemLanguage.Slovak: 
                    return "sk";
                case SystemLanguage.Slovenian: 
                    return "sl";
                case SystemLanguage.Spanish: 
                    return "es";
                case SystemLanguage.Swedish: 
                    return "sv";
                case SystemLanguage.Thai: 
                    return "th";
                case SystemLanguage.Turkish: 
                    return "tr";
                case SystemLanguage.Ukrainian: 
                    return "uk";
                case SystemLanguage.Vietnamese: 
                    return "vi";
                default: return "en";
            }
        }

        public static string ConvertDoubleNullableToString(double? _value)
        {
            if (_value != null)
            {
                return ConvertDoubleNullableToDouble(_value).ToString("R50");
            }
            return "";
        }

        public static string ConvertIntegerNullableToString(int? _value)
        {
            if (_value != null)
            {
                return _value.ToString();
            }
            return "";
        }


        public static double ConvertDoubleNullableToDouble(double? _value)
        {
            try
            {
                if (_value != null)
                {
                    return (double)(_value);
                }
            }
            catch (Exception e)
            {

            }

            return 0.0;
        }

        public static int ConvertIntegerNullableToInteger(int? _value)
        {
            try
            {
                if (_value != null)
                {
                    return (int)(_value);
                }
            }
            catch (Exception e)
            {

            }

            return 0;
        }
    }
}