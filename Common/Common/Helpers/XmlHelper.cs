using Common.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common.Helpers
{
    public class XmlHelper
    {
        public virtual T DeserializeConfiguration<T>(string xmlFilePath)
        {
            T returnObject = default(T);
            if (string.IsNullOrEmpty(xmlFilePath)) return default(T);

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (var xmlStream = new StreamReader(xmlFilePath))
                {
                    returnObject = (T)serializer.Deserialize(xmlStream);
                }
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
                throw;
            }
            return returnObject;
        }
    }
}
