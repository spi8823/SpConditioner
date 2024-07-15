using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpConditioner
{
    public interface IVariableAccessor
    {
        bool GetBool(string key);
        void SetBool(string key, bool value);
        double GetDouble(string key);
        void SetDouble(string key, double value);
    }
}
