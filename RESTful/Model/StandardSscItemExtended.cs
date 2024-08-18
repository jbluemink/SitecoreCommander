
using System.Text.Json.Serialization;

namespace SitecoreCommander.RESTful.Model
{
    internal class StandardSscItemExtended
    {

        public Guid ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemPath { get; set; }
        public Guid ParentID { get; set; }
        public Guid TemplateID { get; set; }
        public string TemplateName { get; set; }
        public string ItemLanguage { get; set; }
        public string ItemVersion { get; set; }

        [JsonPropertyName("__Display name")]
        public string __DisplayName { get; set; }
        public string HasChildren { get; set; }
        public string ItemIcon { get; set; }
        public string ItemMedialUrl { get; set; }
        public string __Sortorder { get; set; }
        [JsonPropertyName("__Created by")]
        public string __CreatedBy { get; set; }
        public string __Created { get; set; }
        [JsonPropertyName("__Never publish")]
        public string __NeverPublish { get; set; }
        public string __Revision { get; set; }
        public string __Hidden { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        [JsonPropertyName("NavigationTitle")]
        public string LinkCaptionInNavigation { get; set; }

        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string OpenGraphImageUrl { get; set; }
        public string SxaTags { get; set; }

        public string __Renderings { get; set; }
        [JsonPropertyName("__Final Renderings")]
        public string __FinalRenderings { get; set; }

        //set programmicaly you can set this when you build a content migrator.
        public string MigratedRenderingenToXmCloud { get; set; }

        //news specific fields
        //Depend on your Custom fields in Sitecore and wich template,fields you want to support
        public string Author { get; set; }

        [JsonPropertyName("Publication Date")]
        public string PublicationDate { get; set; }

        //Section Settings
        [JsonPropertyName("Background Image")]
        public string BackgroundImage { get; set; }


        //components - content block
        public string Body { get; set; }
        public string Link { get; set; }
        public string Image { get; set; }


        //components - video
        public string ExternalID { get; set; }
        public string Videoprovider { get; set; }

        //components - contact/portrait photo
        public string Name { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string LinkedIn { get; set; }
        public string SharedImage { get; set; }

        //components - banner
        public string RichTextTitle { get; set; }

        //contenttoken
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
