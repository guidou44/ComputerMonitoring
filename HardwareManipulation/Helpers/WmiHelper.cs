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
        public virtual T GetWmiValue<T>(string wmiPath, string wmiKey)
        {
            var wmiObject = new ManagementObjectSearcher($"select * from {wmiPath}");


            var value = wmiObject.Get().Cast<ManagementObject>()
                                    .Select(mo => mo[wmiKey].ToString()).FirstOrDefault();
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(value);

        }

        public virtual T GetWmiValue<T>(string wmiPath, string wmiKey, string scope)
        {
            var wmiObject = new ManagementObjectSearcher(scope, $"select * from {wmiPath}");


            var value = wmiObject.Get().Cast<ManagementObject>()
                                    .Select(mo => mo[wmiKey].ToString()).FirstOrDefault();
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(value);

        }
    }
}
