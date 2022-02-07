using System;
using System.Collections;
using System.Collections.Generic;
using TutoTOONS.Utils.Debug.Console;
using UnityEngine;


namespace TutoTOONS
{
    public class IntegrationTestController : MonoBehaviour
    {
        private List<IntegrationTest> integration_tests = new List<IntegrationTest>();
        private float timer = 5f;
        private float next_timer = 10f;
        private bool all_tests_passed;

        private void Start()
        {
            integration_tests = GetTests();
        }

        private void Update()
        {
            if (all_tests_passed)
            {
                return;
            }

            timer -= Time.unscaledDeltaTime;
            DebugConsole.instance.console.content_integration_tests?.UpdateTimer(timer);
            if (0 <= timer)
            {
                return;
            }

            timer = next_timer;
            next_timer *= 2;
            all_tests_passed = true;

            List<IntegrationTestResult> _scripting_symbols = new List<IntegrationTestResult>();
            _scripting_symbols.Add(new IntegrationTestResult() { text = "Scripting Define Symbols", result = true });

            foreach (IntegrationTest item in integration_tests)
            {
                if (!item.test_passed)
                {
                    try
                    {
                        List<IntegrationTestResult> _result = item.TestIntegration();
                        all_tests_passed = _result[0].result && all_tests_passed;
                        DebugConsole.instance.console.content_integration_tests.UpdateTestResult(_result);

                        _scripting_symbols.AddRange(item.GetScriptingSymbolsResults());
                    }
                    catch (Exception e)
                    {
                        IntegrationTestResult _test_name = new IntegrationTestResult();
                        _test_name.result = false;
                        if (item != null && item.name != null)
                        {
                            _test_name.text = item.name;
                        }
                        else
                        {
                            _test_name.text = $"Failed test";
                        }

                        List<IntegrationTestResult> _result = new List<IntegrationTestResult>();
                        _result.Add(_test_name);
                        _result.Add(new IntegrationTestResult() { text = e.ToString(), result = false });
                        DebugConsole.instance.console.content_integration_tests?.UpdateTestResult(_result);
                        all_tests_passed = false;
                    }
                }
            }

            for (int i = 1; i < _scripting_symbols.Count; i++)
            {
                if (_scripting_symbols[i].result == false)
                {
                    _scripting_symbols[0].result = false;
                    break;
                }
            }
            DebugConsole.instance.console.content_integration_tests?.UpdateTestResult(_scripting_symbols);

            if (all_tests_passed)
            {
                timer = -1;
                DebugConsole.instance.console.content_integration_tests?.UpdateTimer(timer);
            }
        }

        private List<IntegrationTest> GetTests()
        {

            TextAsset _asset = Resources.Load<TextAsset>("Tutotoons/integration_tests");
            if (_asset == null)
            {
                Debug.Log("No test integration data file found!");
                return new List<IntegrationTest>();
            }
            IntegrationTestDataRoot _data = JsonUtility.FromJson<IntegrationTestDataRoot>(_asset.text);
            List<IntegrationTest> _result = new List<IntegrationTest>();
            
            _result.Add(new CompliantIntegrationTest() { name = "Compliant Test", integration_data = _data });

            foreach (IntegrationTestData item in _data.data)
            {
                if (!item.enabled || string.IsNullOrEmpty(item.test_class))
                {
                    continue;
                }
                string _test_class = item.test_class;
                System.Type _wrapper_class = System.Type.GetType(_test_class);

                if (_wrapper_class != null)
                {
                    IntegrationTest _test = (IntegrationTest)System.Activator.CreateInstance(_wrapper_class);
                    if (_test != null)
                    {
                        _test.integration_data = _data;
                        _result.Add(_test);
                    }
                }
                else
                {
                    IntegrationTest _test = new IntegrationTest
                    {
                        name = item.test_class,
                        class_not_found = true
                    };
                    _test.integration_data = _data;
                    _result.Add(_test);
                }
            }            
            return _result;
        }
    }
}
