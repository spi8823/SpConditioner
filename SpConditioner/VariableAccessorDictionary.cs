using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpConditioner
{
    public class VariableDictionary : Dictionary<string, object>, IVariableAccessor
    {
        public bool GetBool(string key)
        {
            if (!ContainsKey(key))
                return false;

            var value = this[key];
            if (value is bool b)
                return b;
            else
                return false;
        }

        public double GetDouble(string key)
        {
            if (!ContainsKey(key))
                return 0;

            var value = this[key];
            if (value is double d)
                return d;
            else if (value is float f)
                return f;
            else if (value is int i)
                return i;
            else
                return 0;
        }

        public void SetBool(string key, bool value)
        {
            this[key] = value;
        }

        public void SetDouble(string key, double value)
        {
            this[key] = value;
        }
    }
}
