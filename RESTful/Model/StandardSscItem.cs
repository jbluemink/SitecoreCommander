using System.Text.Json.Serialization;

namespace SitecoreCommander.RESTful.Model
{
    internal class StandardSscItem
    {
        public Guid ItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemPath { get; set; } = string.Empty;
        public Guid ParentID { get; set; }
        public Guid TemplateID { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string ItemLanguage { get; set; } = string.Empty;
        public string ItemVersion { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string HasChildren { get; set; } = string.Empty;
        public string ItemIcon { get; set; } = string.Empty;
        public string ItemMedialUrl { get; set; } = string.Empty;
        public string __Sortorder { get; set; } = string.Empty;
        [JsonPropertyName("_Created by")]
        public string __CreatedBy { get; set; } = string.Empty;
        public string __Created { get; set; } = string.Empty;
    }
}
