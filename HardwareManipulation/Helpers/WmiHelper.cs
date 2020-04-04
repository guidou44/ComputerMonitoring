using Common.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace HardwareAccess.Helpers
{
    public class WmiHelper
    {
        public T GetWmiValue<T>(string wmiPath, string wmiKey, string scope = null)
        {
            var wmiObject = (scope != null) ? new ManagementObjectSearcher(scope, $"select * from {wmiPath}") :
                                              new ManagementObjectSearcher($"select * from {wmiPath}");

            try
            {
                var value = wmiObject.Get().Cast<ManagementObject>()
                                     .Select(mo => mo[wmiKey].ToString()).FirstOrDefault();
                if (value == null) throw new ArgumentException($"Could not find wmiObject for key {wmiKey}");
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (converter != null) ? (T)converter.ConvertFromString(value) :
                    throw new ArgumentException($"Cannot convert tpye {typeof(T).Name} to string.");
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
