using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Common.Exceptions;

namespace Common.Helpers
{
    public class XmlHelper
    {
        public virtual T Deserialize<T>(string path)
        {
            T returnObject = default(T);
            if (string.IsNullOrEmpty(path)) return default(T);

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (var xmlStream = new StreamReader(path))
                {
                    returnObject = (T)serializer.Deserialize(xmlStream);
                }
            }
            catch (Exception e)
            {
                throw new XmlDeserializationException($"Path: {path}\n{e.Message}");
            }
            return returnObject;
        }

        public void SerializeOverwrite<T>(T serializable, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (XmlWriter writer = XmlWriter.Create(stream))
                    {
                        serializer.Serialize(writer, serializable);
                    }
                }
            }
            catch (Exception e)
            {
                throw new XmlSerializationException($"Path: {path}\n{e.Message}");
            }
        }
    }
}