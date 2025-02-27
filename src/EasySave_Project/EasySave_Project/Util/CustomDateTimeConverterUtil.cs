using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave_Project.Util;

public class CustomDateTimeConverterUtil: JsonConverter<DateTime>
{
    private readonly string _format = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Reads and converts a JSON string to a DateTime object.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader to read from.</param>
    /// <param name="typeToConvert">The target type (should be DateTime).</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>The parsed DateTime object.</returns>
    /// <exception cref="JsonException">Thrown if parsing fails.</exception>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (DateTime.TryParseExact(dateString, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return date;
        }
        throw new JsonException($"Unable to convert \"{dateString}\" to DateTime with format {_format}.");
    }
    
    /// <summary>
    /// Writes a DateTime object as a JSON string using the specified format.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The DateTime value to serialize.</param>
    /// <param name="options">Serialization options.</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}