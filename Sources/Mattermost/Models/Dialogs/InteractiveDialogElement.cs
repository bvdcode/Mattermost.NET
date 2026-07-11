using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Input element displayed inside an interactive dialog.
    /// </summary>
    public class InteractiveDialogElement
    {
        /// <summary>
        /// Display name shown to the user.
        /// </summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Element name used as the submission key.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Element type.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InteractiveDialogElementType? Type { get; set; }

        /// <summary>
        /// Text input subtype.
        /// </summary>
        [JsonPropertyName("subtype")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InteractiveDialogTextSubtype? Subtype { get; set; }

        /// <summary>
        /// Default submitted value.
        /// </summary>
        [JsonPropertyName("default")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Placeholder text shown before input.
        /// </summary>
        [JsonPropertyName("placeholder")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Placeholder { get; set; }

        /// <summary>
        /// Help text displayed below the element.
        /// </summary>
        [JsonPropertyName("help_text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? HelpText { get; set; }

        /// <summary>
        /// Whether the element can be omitted by the user.
        /// </summary>
        [JsonPropertyName("optional")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Optional { get; set; }

        /// <summary>
        /// Minimum text length.
        /// </summary>
        [JsonPropertyName("min_length")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinLength { get; set; }

        /// <summary>
        /// Maximum text length.
        /// </summary>
        [JsonPropertyName("max_length")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxLength { get; set; }

        /// <summary>
        /// Static select or radio options.
        /// </summary>
        [JsonPropertyName("options")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<InteractiveDialogOption>? Options { get; set; }

        /// <summary>
        /// Data source for generated select options.
        /// </summary>
        [JsonPropertyName("data_source")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InteractiveDialogDataSource? DataSource { get; set; }

        /// <summary>
        /// URL used for dynamic select lookups.
        /// </summary>
        [JsonPropertyName("data_source_url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DataSourceUrl { get; set; }

        /// <summary>
        /// Whether a select element supports multiple values.
        /// </summary>
        [JsonPropertyName("multiselect")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Multiselect { get; set; }

        /// <summary>
        /// Whether changes to this element trigger dialog refresh.
        /// </summary>
        [JsonPropertyName("refresh")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Refresh { get; set; }

        /// <summary>
        /// Date and datetime picker configuration.
        /// </summary>
        [JsonPropertyName("datetime_config")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public InteractiveDialogDateTimeConfig? DateTimeConfig { get; set; }

        /// <summary>
        /// Whether a file element allows multiple uploads.
        /// </summary>
        [JsonPropertyName("allow_multiple")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowMultiple { get; set; }

        /// <summary>
        /// Action button configuration.
        /// </summary>
        [JsonPropertyName("action_button")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public InteractiveDialogActionButton? ActionButton { get; set; }
    }
}
