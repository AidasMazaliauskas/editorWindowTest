using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TutoTOONS
{
    public class IntegrationTestEditor
    {
        private static Dictionary<string, string> tests_classes = new Dictionary<string, string>()
        {
            //name of BuildArguments.cs class fields, not build arguments 
            {nameof(BuildArguments.admob_mediation), "TutoTOONS.AdMobTest"},
            {nameof(BuildArguments.applovin_max), "TutoTOONS.ApplovinMaxTest"},
            {nameof(BuildArguments.builder), "TutoTOONS.BuilderTest"},
            {nameof(BuildArguments.chartboost), "TutoTOONS.ChartboostTest"},
            {nameof(BuildArguments.firebase_analytics), "TutoTOONS.FirebaseAnalyticsTest"},
            {nameof(BuildArguments.firebase_auth), "TutoTOONS.FirebaseAuthTest"},
            {nameof(BuildArguments.firebase_crashlytics), "TutoTOONS.FirebaseCrashlyticsTest"},
            {nameof(BuildArguments.firebase_database), "TutoTOONS.FirebaseRealtimeDatabaseTest"},
            {nameof(BuildArguments.firebase_messaging), "TutoTOONS.FirebaseMessagingTest"},
            {nameof(BuildArguments.freetime), "TutoTOONS.FreetimeTest"},
            {nameof(BuildArguments.huawei_ads), "TutoTOONS.HuaweiAdsTest"},
            {nameof(BuildArguments.kidoz), "TutoTOONS.KidozTest"},
            {nameof(BuildArguments.singular), "TutoTOONS.SingularTest"},
            {nameof(BuildArguments.soomla),"TutoTOONS.SoomlaTest"},
            {nameof(BuildArguments.super_awesome), "TutoTOONS.SuperAwesomeTest"},
            {nameof(BuildArguments.adolf), "TutoTOONS.TutoAdsTest"},
            {nameof(BuildArguments.intro), "TutoTOONS.TutoToonsIntroTest"},
            {nameof(BuildArguments.subscription), "TutoTOONS.TutoToonsSubscriptionTest"},
            {nameof(BuildArguments.purchasing_package), "TutoTOONS.PurchasingTest"},
            {nameof(BuildArguments.ironsource_mediation), "TutoTOONS.IronSourceTest"},
            {nameof(BuildArguments.ironsource_mediation_facebook), "TutoTOONS.FacebookTest"}

        };

        static string GetTestClass(string _key)
        {
            if (tests_classes.ContainsKey(_key))
            {
                return tests_classes[_key];
            }
            return "";
        }

        public static void GenerateTestData(BuildArguments _arguments)
        {
            string file_path = Application.dataPath + "/Resources/Tutotoons";
            string file_name = "/integration_tests.json";

            if (!Directory.Exists(file_path))
            {
                Directory.CreateDirectory(file_path);
            }

            if (File.Exists(file_path + file_name))
            {
                File.Delete(file_path + file_name);
            }

            IntegrationTestDataRoot root = new IntegrationTestDataRoot();
            root.data = new List<IntegrationTestData>();

            Type _type = _arguments.GetType();

            foreach (FieldInfo _field in _type.GetFields())
            {
                if (_field.FieldType == typeof(bool))
                {
                    root.data.Add(new IntegrationTestData()
                    {
                        argument = _field.Name,
                        test_class = GetTestClass(_field.Name),
                        enabled = (bool)_field.GetValue(_arguments)
                    }); ;
                }
            }
            File.WriteAllText(file_path + file_name, JsonUtility.ToJson(root));
        }
    }
}