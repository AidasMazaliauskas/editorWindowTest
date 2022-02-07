using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace TutoTOONS
{
    public class ABTesting
    {
        public const int STATE_DISABLED = 0;
        public const int STATE_INITIALIZING = 1;
        public const int STATE_ENABLED = 2;
        public static int state = STATE_DISABLED;

        public static int test_id;
        public static int variant_id;
        public static string variant_parameters;

        public static void Init()
        {
            if (SavedData.first_run)
            {
                state = STATE_INITIALIZING;
            }
            else
            {
                test_id = SavedData.GetInt("ab_test_id", 0);
                if(test_id == 0)
                {
                    state = STATE_DISABLED;
                }
                else
                {
                    state = STATE_ENABLED;
                    variant_id = SavedData.GetInt("ab_test_variant_id");
                    variant_parameters = SavedData.GetString("ab_test_variant_parameters");
                }
            }
            //Debug.Log("AB testing first run: " + SavedData.first_run + ", test ID: " + test_id);
        }

        static void AssignTest()
        {
            if(AppConfig.settings.ab_tests == null)
            {
                state = STATE_DISABLED;
                return;
            }
			if (AppConfig.settings.ab_tests.tests == null)
            {
                state = STATE_DISABLED;
                return;
            }

            //Check for country match
            string _countries_str = AppConfig.settings.ab_tests.enabled_countries;
            if (_countries_str != null && _countries_str != "ALL")
            {
                string[] _countries = _countries_str.Split(';');
                bool _country_found = false;
                for(int i = 0; i < _countries.Length; i++)
                {
                    if(_countries[i] == AppConfig.info.country)
                    {
                        _country_found = true;
                        break;
                    }
                }
                if(!_country_found)
                {
                    state = STATE_DISABLED;
                    return;
                }
            }

            //Assign random test, taking into account test percentage
            System.Random rand_gen = new System.Random();
            double rand_num = rand_gen.NextDouble();
            double percentage_sum = 0.0;
            for(int i = 0; i < AppConfig.settings.ab_tests.tests.Count; i++)
            {
                AppConfigABTest _test = AppConfig.settings.ab_tests.tests[i];
                percentage_sum += _test.percentage;
                if(rand_num < percentage_sum)
                {
                    //Select this test
                    test_id = _test.test_id;
                    int _variant_num = rand_gen.Next(_test.variants.Count);
                    AppConfigABTestVariant _variant = _test.variants[_variant_num];
                    variant_id = _variant.variant_id;
                    variant_parameters = _variant.parameters;
                    SavedData.SetInt("ab_test_id", test_id);
                    SavedData.SetInt("ab_test_variant_id", variant_id);
                    SavedData.SetString("ab_test_variant_parameters", variant_parameters);
                    state = STATE_ENABLED;
                    Debug.Log("ABTesting selected test: " + test_id + ", variant: " + variant_id);
                    return;
                }
            }
            state = STATE_DISABLED;
        }

        public static void Update()
        {
            switch (state)
            {
                case STATE_INITIALIZING:
                    if (AppConfig.state == AppConfig.STATE_LOADED)
                    {
                        AssignTest();
                    }
                    break;
            }
        }
    }
}
