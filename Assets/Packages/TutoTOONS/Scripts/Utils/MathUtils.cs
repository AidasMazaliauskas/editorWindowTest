using System;
using System.Globalization;
using UnityEngine;

namespace TutoTOONS
{
    public class MathUtils
    {
        private static CultureInfo culture_info;

        public static double ParseDouble(string _double_string, double _default_val = 0.0)
        {
            if (_double_string.Contains(".") && _double_string.Contains(","))
            {
                try
                {
                    double _parsed_double = Double.Parse(_double_string);
                    return _parsed_double;
                }
                catch (Exception e)
                {
                    Debug.Log("ParseDouble parse failed with error: " + e.Message);
                }

                // Try parse with latest working culture
                try
                {
                    if (culture_info != null)
                    {
                        double _parsed_double = Double.Parse(_double_string, culture_info);
                        return _parsed_double;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("ParseDouble parse failed with error: " + e.Message);
                }

                // Try parse with all possible cultures
                foreach (CultureInfo _culture_info in CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    try
                    {
                        double _parsed_double = Double.Parse(_double_string, _culture_info);
                        culture_info = _culture_info;
                        return _parsed_double;
                    }
                    catch (Exception e)
                    {
                        Debug.Log("ParseDouble parse failed with error: " + e.Message);
                    }
                }
            }
            else
            {
                try
                {
                    _double_string = _double_string.Replace(',', '.');
                    double _parsed_double = Double.Parse(_double_string, NumberStyles.Any, CultureInfo.InvariantCulture);
                    return _parsed_double;
                }
                catch (Exception e)
                {
                    Debug.Log("ParseDouble parse failed with error: " + e.Message);
                }
            }

            return _default_val;
        }

        public static float ParseFloat(string _float_string, float _default_val = 0.0f)
        {
            if (_float_string.Contains(".") && _float_string.Contains(","))
            {
                try
                {
                    float _parsed_float = float.Parse(_float_string);
                    return float.IsNaN(_parsed_float)? _default_val : _parsed_float;
                }
                catch (Exception e)
                {
                    Debug.Log("ParseFloat parse failed with error: " + e.Message);
                }

                // Try parse with latest working culture
                try
                {
                    if (culture_info != null)
                    {
                        float _parsed_float = float.Parse(_float_string, culture_info);
                        return float.IsNaN(_parsed_float) ? _default_val : _parsed_float;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("ParseFloat parse failed with error: " + e.Message);
                }

                // Try parse with all possible cultures
                foreach (CultureInfo _culture_info in CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    try
                    {
                        float _parsed_float = float.Parse(_float_string, _culture_info);
                        culture_info = _culture_info;
                        return float.IsNaN(_parsed_float) ? _default_val : _parsed_float;
                    }
                    catch (Exception e)
                    {
                        Debug.Log("ParseFloat parse failed with error: " + e.Message);
                    }
                }
            }
            else
            {
                try
                {
                    _float_string = _float_string.Replace(',', '.');
                    float _parsed_float = float.Parse(_float_string, NumberStyles.Any, CultureInfo.InvariantCulture);
                    return float.IsNaN(_parsed_float) ? _default_val : _parsed_float;
                }
                catch (Exception e)
                {
                    Debug.Log("ParseFloat parse failed with error: " + e.Message);
                }
            }

            return _default_val;
        }
    }
}