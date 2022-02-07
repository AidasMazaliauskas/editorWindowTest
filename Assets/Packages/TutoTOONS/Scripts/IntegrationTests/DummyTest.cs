using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    public class DummyTest : IntegrationTest
    {
        public DummyTest()
        {
            name = "Dummy Test";
            scripting_define_symbols.Add("GEN_DEBUG");
            needed_scripting_symbols.Add("GEN_DEBUG");
        }

        protected override bool CheckIfConfigsWereLoaded()
        {
            network_ids.Clear();
            network_ids.Add("DEBUG KEY!");
            return true;
        }

        protected override bool CheckIfPackagesWereAdded()
        {
            if (Time.time< 10f)
            {
                throw new System.Exception("Test exception");
            }
            return true;
        }

        protected override bool CheckIfPackagesInitialised()
        {
            if (Time.time > 25f)
            {
                return true;
            }
            return false;
        }

        protected override bool CheckIfScriptingSymbolsWereAdded()
        {
            return true;
        }
    }
}