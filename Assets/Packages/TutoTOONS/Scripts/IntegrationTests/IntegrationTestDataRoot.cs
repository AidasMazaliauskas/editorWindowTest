using System.Collections.Generic;
using System;

namespace TutoTOONS
{
    [Serializable]
    public class IntegrationTestDataRoot
    {
        public List<IntegrationTestData> data;
    }

    [Serializable]
    public class IntegrationTestData
    {
        public string argument;
        public string test_class;
        public bool enabled;
    }

    public class IntegrationTestResult
    {
        public string text;
        public bool result;
    }
}