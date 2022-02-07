using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    public class CompliantIntegrationTest : IntegrationTest
    {
        public override List<IntegrationTestResult> TestIntegration()
        {
            List<IntegrationTestResult> _result = new List<IntegrationTestResult>();
            IntegrationTestResult _base_result = new IntegrationTestResult();
            _result.Add(_base_result);

            bool _is_sugarfree = AppConfig.account == AppConfig.ACCOUNT_SUGARFREE;
            bool _is_tuto_on_ios = Application.platform == RuntimePlatform.IPhonePlayer && AppConfig.account == AppConfig.ACCOUNT_TUTOTOONS;

            if (ParameterEnabled("ironsource_mediation_pangle"))
            {                
                if (_is_sugarfree == false && _is_tuto_on_ios == false)
                {
                    _result.Add(new IntegrationTestResult() { text = "Make sure IS Pangle is only allowed on Sugarfree games and TutoTOONS games for iOS", result = false });
                }
            }

            if (ParameterEnabled("iron_source_mediation_facebook"))
            {
                if (_is_sugarfree)
                {
                    _result.Add(new IntegrationTestResult() { text = "Make sure IS Facebook is only enabled on Sugarfree games", result = false });
                }
            }

            if (_result.Count > 1)
            {
                _base_result.text = "Compliant test failed";
                _base_result.result = false;
            }
            else
            {
                _base_result.text = "Compliant test passed";
                _base_result.result = true;
            }
            return _result;
        }

        private bool ParameterEnabled(string _argument)
        {
            foreach (IntegrationTestData _data in integration_data.data)
            {
                if (_data.argument == _argument)
                {
                    return _data.enabled;
                }
            }
            return false;
        }
    }
}
