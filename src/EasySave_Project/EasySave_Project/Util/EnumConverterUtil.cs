using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave_Project.Util;

public class EnumConverterUtil
{
    /// <summary>
    /// Custom JSON converter for serializing and deserializing enums.
    /// </summary>
    public class JsonEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        /// <summary>
        /// Reads and converts a JSON string to an enum value.
        /// </summary>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string enumValue = reader.GetString();
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
            {
                if (string.IsNullOrEmpty(enumValue))
                {
                    return default(T);
                }
            }

            if (Enum.TryParse(enumValue, ignoreCase: true, out T result))
            {
                return result;
            }
            else
            {
                throw new JsonException($"Unable to convert '{enumValue}' to {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Writes an enum value as a JSON string in uppercase.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpper());
        }
    }

}