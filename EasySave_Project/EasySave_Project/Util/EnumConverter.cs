using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave_Project.Util;

public class EnumConverter
{
    public class JsonEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string enumValue = reader.GetString();

            // Vérification si T est un type nullable
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
            {
                // Si T est nullable, nous allons vérifier si l'élément JSON est null
                if (string.IsNullOrEmpty(enumValue))
                {
                    return default(T); // Renvoie null pour les types nullable
                }
            }

            // Désérialisation des énumérations pour les types non nullable
            if (Enum.TryParse(enumValue, ignoreCase: true, out T result))
            {
                return result;
            }
            else
            {
                throw new JsonException($"Unable to convert '{enumValue}' to {typeof(T).Name}");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpper());
        }
    }

}