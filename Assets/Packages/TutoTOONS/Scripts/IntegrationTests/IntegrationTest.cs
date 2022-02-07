using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    public class IntegrationTest
    {
        public string name;
        public bool should_be_enabled;
        public bool class_not_found = false;
        public bool test_passed = false;
        public List<string> scripting_define_symbols;
        public List<string> needed_scripting_symbols;
        public List<string> network_ids;
        public IntegrationTestDataRoot integration_data;

        public IntegrationTest()
        {
            scripting_define_symbols = new List<string>();
            needed_scripting_symbols = new List<string>();
            network_ids = new List<string>();
        }

        public virtual List<IntegrationTestResult> TestIntegration()
        {
            List<IntegrationTestResult> _result = new List<IntegrationTestResult>();
            network_ids = new List<string>();

            bool _all_passed = true;
            _result.Add(new IntegrationTestResult() { text = name });
            if (class_not_found)
            {
                _result.Add(new IntegrationTestResult() { text = "Could not find test class", result = false });
                _result[0].result = false;
                return _result;
            }
            if (!CheckIfPackagesWereAdded())
            {
                _result.Add(new IntegrationTestResult() { text = "Packages were not added", result = false });
                _all_passed = false;
            }
            else
            {
                _result.Add(new IntegrationTestResult() { text = "Packages were added", result = true });
            }

            if (!CheckIfScriptingSymbolsWereAdded())
            {
                _result.Add(new IntegrationTestResult() { text = "Scripting symbols were not added", result = false });
                _all_passed = false;
            }
            else
            {
                _result.Add(new IntegrationTestResult() { text = "Scripting symbols were added", result = true });
            }

            if (!CheckIfConfigsWereLoaded())
            {
                _result.Add(new IntegrationTestResult() { text = "Configs were not loaded", result = false });
                _all_passed = false;
            }
            else
            {
                string _network_id = "";
                if (network_ids.Count > 0)
                {
                    _network_id += "\nNetwork Ids:";
                    for (int i = 0; i < network_ids.Count; i++)
                    {
                        _network_id += "\n" + network_ids[i];
                    }
                }
                _result.Add(new IntegrationTestResult() { text = "Configs were loaded" + _network_id, result = true });
            }

            if (!CheckIfPackagesInitialised())
            {
                _result.Add(new IntegrationTestResult() { text = "Packages were not initialised", result = false });
                _all_passed = false;
            }
            else
            {
                _result.Add(new IntegrationTestResult() { text = "Packages were initialised", result = true });
            }
            _result[0].result = _all_passed;
            return _result;

        }

        public List<IntegrationTestResult> GetScriptingSymbolsResults()
        {
            List<IntegrationTestResult> _result = new List<IntegrationTestResult>();

            foreach (string _symbol in needed_scripting_symbols)
            {
                IntegrationTestResult _symbol_result = new IntegrationTestResult()
                {
                    text = _symbol,
                    result = scripting_define_symbols.Contains(_symbol)
                };
                _result.Add(_symbol_result);
            }


            return _result;
        }

        protected virtual bool CheckIfPackagesWereAdded()
        {
            return true;
        }
        protected virtual bool CheckIfScriptingSymbolsWereAdded()
        {
            return true;
        }
        protected virtual bool CheckIfConfigsWereLoaded()
        {
            return true;
        }
        protected virtual bool CheckIfPackagesInitialised()
        {
            return true;
        }
    }
}