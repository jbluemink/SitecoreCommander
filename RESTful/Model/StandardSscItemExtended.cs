
using System.Text.Json.Serialization;

namespace SitecoreCommander.RESTful.Model
{
    internal class StandardSscItemExtended
    {

        public Guid ItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemPath { get; set; } = string.Empty;
        public Guid ParentID { get; set; }
        public Guid TemplateID { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string ItemLanguage { get; set; } = string.Empty;
        public string ItemVersion { get; set; } = string.Empty;

        [JsonPropertyName("__Display name")]
        public string __DisplayName { get; set; } = string.Empty;
        public string HasChildren { get; set; } = string.Empty;
        public string ItemIcon { get; set; } = string.Empty;
        public string ItemMedialUrl { get; set; } = string.Empty;
        public string __Sortorder { get; set; } = string.Empty;
        [JsonPropertyName("__Created by")]
        public string __CreatedBy { get; set; } = string.Empty;
        public string __Created { get; set; } = string.Empty;
        [JsonPropertyName("__Never publish")]
        public string __NeverPublish { get; set; } = string.Empty;
        public string __Revision { get; set; } = string.Empty;
        public string __Hidden { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        [JsonPropertyName("NavigationTitle")]
        public string LinkCaptionInNavigation { get; set; } = string.Empty;

        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string OpenGraphImageUrl { get; set; } = string.Empty;
        public string SxaTags { get; set; } = string.Empty;

        public string __Renderings { get; set; } = string.Empty;
        [JsonPropertyName("__Final Renderings")]
        public string __FinalRenderings { get; set; } = string.Empty;

        //set programmicaly you can set this when you build a content migrator.
        public string MigratedRenderingenToXmCloud { get; set; } = string.Empty;

        //news specific fields
        //Depend on your Custom fields in Sitecore and wich template,fields you want to support
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("Publication Date")]
        public string PublicationDate { get; set; } = string.Empty;

        //Section Settings
        [JsonPropertyName("Background Image")]
        public string BackgroundImage { get; set; } = string.Empty;


        //components - content block
        public string Body { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;


        //components - video
        public string ExternalID { get; set; } = string.Empty;
        public string Videoprovider { get; set; } = string.Empty;

        //components - contact/portrait photo
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string LinkedIn { get; set; } = string.Empty;
        public string SharedImage { get; set; } = string.Empty;

        //components - banner
        public string RichTextTitle { get; set; } = string.Empty;

        //contenttoken
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
