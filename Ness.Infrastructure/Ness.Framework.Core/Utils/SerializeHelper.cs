using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Framework.Core.Utils
{
    public class SerializeHelper
    {
        /// <summary>
        /// Deserialize from xml file to a specific type object
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static T DeserializeObjectFromPath<T>(string xmlPath)
        {
            T desObj = default(T);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                using (FileStream reader = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    desObj = (T)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Cant DeserializeObjectFromPath of object {0}", typeof(T)));
            }


            return desObj;

        }
        
        /// <summary>
        /// Deserialize from xml string to a specific type object
        /// </summary>
        /// <param name="xmlString"></param>
        /// <param name="rootAttribute"></param>
        /// <returns></returns>
        public static T DeserializeObjectFromString<T>(string xmlString, string rootAttribute)
        {
            T desObj = default(T);
            try
            {
                var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootAttribute));

                using (TextReader reader = new StringReader(xmlString))
                {
                    desObj = (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Cant DeserializeObjectFromString of object {0}", typeof(T)));
            }

            return desObj;

        }

        public static object DeserializeObjectFromString(Type type, string xmlString, string rootAttribute)
        {
            object desObj = null;
            try
            {
                var serializer = new XmlSerializer(type, new XmlRootAttribute(rootAttribute));

                using (TextReader reader = new StringReader(xmlString))
                {
                    desObj = serializer.Deserialize(reader);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Cant DeserializeObjectFromString of object {0}", type));
            }

            return desObj;
        }
        
        /// <summary>
        /// Deserialize from Stream Reader to a specific type object
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static T DeserializeObjectFromReader<T>(XmlReader xmlReader)
        {
            T desObj = default(T);
            try
            {
                var serializer = new XmlSerializer(typeof(T));

                desObj = (T)serializer.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Cant DeserializeObjectFromReader of object {0}", typeof(T)));
            }


            return desObj;

        }
        
        public static XmlNode SerializeObjectToXmlNode<T>(T obj)
        {
            XmlNode resultNode = null;

            try
            {
                if (obj == null)
                    throw new ArgumentNullException("Argument cannot be null");


                var xmlSerializer = new XmlSerializer(typeof(T));
                using (var memoryStream = new MemoryStream())
                {
                    try
                    {
                        xmlSerializer.Serialize(memoryStream, obj);
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                    memoryStream.Position = 0;
                    var doc = new XmlDocument();
                    doc.Load(memoryStream);
                    resultNode = doc.DocumentElement;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Cant SerializeObjectToXmlNode of object {0}", obj.GetType()));
            }

            return resultNode;
        }
        
        /// <summary>
        /// convert an object To string using XmlSerialization
        /// </summary>
        /// <param name="inputObject"></param>
        /// <returns></returns>
        public static string ConvertObjectToXmlString(object inputObject)
        {
            var xs = new XmlSerializer(inputObject.GetType());
            byte[] arr;
            using (var memoryStream = new MemoryStream())
            {
                xs.Serialize(memoryStream, inputObject);
                int count = (int)memoryStream.Length; // saves object in memory stream
                arr = new byte[count];

                memoryStream.Seek(0, SeekOrigin.Begin);
                // copy stream contents in byte array
                memoryStream.Read(arr, 0, count);
            }
            return Encoding.UTF8.GetString(arr);
        }
        
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static string CreateXML(Object YourClassObject)
        {
            XmlDocument xmlDoc = new XmlDocument();   //Represents an XML document, 
            // Initializes a new instance of the XmlDocument class.          
            XmlSerializer xmlSerializer = new XmlSerializer(YourClassObject.GetType());
            // Creates a stream whose backing store is memory. 
            using (MemoryStream xmlStream = new MemoryStream())
            {
                xmlSerializer.Serialize(xmlStream, YourClassObject);
                xmlStream.Position = 0;
                //Loads the XML document from the specified string.
                xmlDoc.Load(xmlStream);
                return xmlDoc.InnerXml;
            }
        }
    }
}
