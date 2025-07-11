using System.Text.Json.Serialization;

namespace SitecoreCommander.Authoring.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CriteriaOperator
    {
        MUST,
        SHOULD,
        MUST_NOT
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CriteriaType
    {
        EXACT,
        STARTSWITH,
        CONTAINS
        // Add other types as needed
    }

    public class SearchCriteriaInput
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("criteriaType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CriteriaType? CriteriaType { get; set; }

        [JsonPropertyName("operator")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CriteriaOperator? Operator { get; set; }
    }
}
