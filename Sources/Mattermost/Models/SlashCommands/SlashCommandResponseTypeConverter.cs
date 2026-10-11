using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    internal class SlashCommandResponseTypeConverter : JsonConverter<SlashCommandResponseType?>
    {
        private static readonly JsonConverter<SlashCommandResponseType> EnumConverter =
            (JsonConverter<SlashCommandResponseType>)new JsonStringEnumConverter().CreateConverter(
                typeof(SlashCommandResponseType), new JsonSerializerOptions());

        public override SlashCommandResponseType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null
                || (reader.TokenType == JsonTokenType.String && reader.GetString() == string.Empty))
            {
                return null;
            }
            return EnumConverter.Read(ref reader, typeof(SlashCommandResponseType), options);
        }

        public override void Write(Utf8JsonWriter writer, SlashCommandResponseType? value, JsonSerializerOptions options)
        {
            if (value is null) { writer.WriteNullValue(); }
            else { EnumConverter.Write(writer, value.Value, options); }
        }
    }
}
