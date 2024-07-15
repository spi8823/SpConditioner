using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpConditioner;

namespace SpConditionerTest
{
    internal class TestVariableAccessor : IVariableAccessor
    {
        public bool GetBool(string key)
        {
            return key switch
            {
                "T" => true,
                "F" => false,
                _ => false,
            };
        }

        public double GetDouble(string key)
        {
            return key switch
            {
                "Zero" => 0,
                "One" => 1,
                "Two" => 2,
                "Three" => 3,
                "Four" => 4,
                "Five" => 5,
                "Six" => 6,
                _ => 0,
            };
        }

        public void SetBool(string key, bool value)
        {
            throw new NotImplementedException();
        }

        public void SetDouble(string key, double value)
        {
            throw new NotImplementedException();
        }
    }
}
