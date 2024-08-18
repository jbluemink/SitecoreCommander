using System.Text.Json.Serialization;

namespace SitecoreCommander.RESTful.Model
{
    internal class StandardSscItem
    {
        public Guid ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemPath { get; set; }
        public Guid ParentID { get; set; }
        public Guid TemplateID { get; set; }
        public string TemplateName { get; set; }
        public string ItemLanguage { get; set; }
        public string ItemVersion { get; set; }
        public string DisplayName { get; set; }
        public string HasChildren { get; set; }
        public string ItemIcon { get; set; }
        public string ItemMedialUrl { get; set; }
        public string __Sortorder { get; set; }
        [JsonPropertyName("_Created by")]
        public string __CreatedBy { get; set; }
        public string __Created { get; set; }
    }
}
