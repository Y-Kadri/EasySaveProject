using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasySave_Library_Log.Utils
{
    /// <summary>
    /// Utility class for serialization and deserialization of objects in JSON and XML formats.
    /// </summary>
    public static class SerializerUtil
    {
        /// <summary>
        /// Serializes the given object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The JSON string representation of the object.</returns>
        public static string SerializeToJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Deserializes the given JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T DeserializeJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Serializes the given object to an XML string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The XML string representation of the object.</returns>
        public static string SerializeToXml<T>(T obj)
        {
            using StringWriter stringWriter = new();
            XmlSerializer serializer = new(typeof(T));
            serializer.Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Deserializes the given XML string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="xml">The XML string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T DeserializeXml<T>(string xml)
        {
            using StringReader stringReader = new(xml);
            XmlSerializer serializer = new(typeof(T));
            return (T)serializer.Deserialize(stringReader);
        }
    }
}
