namespace SitecoreCommander.ContentHub.Model
{
    internal sealed class ContentHubOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string AssetDefinitionName { get; set; } = "M.Asset";
        public string AssetTitlePropertyName { get; set; } = "Title";
        public string AssetAltTextPropertyName { get; set; } = "SC.Asset.AltText";
        public string AssetFileNamePropertyName { get; set; } = "FileName";
        public string AssetDescriptionPropertyName { get; set; } = "Description";

        public string ContentRepositoryRelationName { get; set; } = "ContentRepositoryToAsset";
        public string StandardContentRepositoryIdentifier { get; set; } = "M.Content.Repository.Standard";

        public string FinalLifecycleRelationName { get; set; } = "FinalLifeCycleStatusToAsset";
        public string ApprovedLifecycleIdentifier { get; set; } = "M.Final.LifeCycle.Status.Approved";

        public string CategoryDefinitionName { get; set; } = "M.Category";
        public string CategoryRelationName { get; set; } = "AssetCategoryToAsset";

        public string TaxonomyNamePropertyName { get; set; } = "TaxonomyName";

        public bool DirectBinaryUploadEnabled { get; set; }
        public bool UpdateMediaFileForExistingAssets { get; set; }
        public string UploadConfigurationName { get; set; } = string.Empty;
        public string UploadActionName { get; set; } = string.Empty;
        public string UploadActionAssetIdParameterName { get; set; } = "assetId";
        public bool EnsurePublicLinks { get; set; } = true;
        public bool RequirePublicLinks { get; set; } = true;
        public string PublicLinkActionName { get; set; } = string.Empty;
        public string PublicLinkActionAssetIdParameterName { get; set; } = "assetId";
        public int PublicLinkRetryCount { get; set; } = 3;
        public int PublicLinkRetryDelayMs { get; set; } = 1500;

        public static ContentHubOptions FromEnvironment()
        {
            var options = new ContentHubOptions
            {
                Endpoint = GetConfigValue("ContentHub:Endpoint", string.Empty),
                ClientId = GetConfigValue("ContentHub:ClientId", string.Empty),
                ClientSecret = GetConfigValue("ContentHub:ClientSecret", string.Empty),
                UserName = GetConfigValue("ContentHub:UserName", string.Empty),
                Password = GetConfigValue("ContentHub:Password", string.Empty),
                AssetDefinitionName = GetConfigValue("ContentHub:AssetDefinitionName", "M.Asset"),
                AssetTitlePropertyName = GetConfigValue("ContentHub:AssetTitlePropertyName", "Title"),
                AssetAltTextPropertyName = GetConfigValue("ContentHub:AssetAltTextPropertyName", "SC.Asset.AltText"),
                AssetFileNamePropertyName = GetConfigValue("ContentHub:AssetFileNamePropertyName", "FileName"),
                AssetDescriptionPropertyName = GetConfigValue("ContentHub:AssetDescriptionPropertyName", "Description"),
                ContentRepositoryRelationName = GetConfigValue("ContentHub:ContentRepositoryRelationName", "ContentRepositoryToAsset"),
                StandardContentRepositoryIdentifier = GetConfigValue("ContentHub:StandardContentRepositoryIdentifier", "M.Content.Repository.Standard"),
                FinalLifecycleRelationName = GetConfigValue("ContentHub:FinalLifecycleRelationName", "FinalLifeCycleStatusToAsset"),
                ApprovedLifecycleIdentifier = GetConfigValue("ContentHub:ApprovedLifecycleIdentifier", "M.Final.LifeCycle.Status.Approved"),
                CategoryDefinitionName = GetConfigValue("ContentHub:CategoryDefinitionName", "M.Category"),
                CategoryRelationName = GetConfigValue("ContentHub:CategoryRelationName", "AssetCategoryToAsset"),
                TaxonomyNamePropertyName = GetConfigValue("ContentHub:TaxonomyNamePropertyName", "TaxonomyName"),
                DirectBinaryUploadEnabled = bool.TryParse(GetConfigValue("ContentHub:DirectBinaryUploadEnabled", "false"), out var upload) && upload,
                UpdateMediaFileForExistingAssets = bool.TryParse(GetConfigValue("ContentHub:UpdateMediaFileForExistingAssets", "false"), out var update) && update,
                UploadConfigurationName = GetConfigValue("ContentHub:UploadConfigurationName", string.Empty),
                UploadActionName = GetConfigValue("ContentHub:UploadActionName", string.Empty),
                UploadActionAssetIdParameterName = GetConfigValue("ContentHub:UploadActionAssetIdParameterName", "assetId"),
                EnsurePublicLinks = bool.TryParse(GetConfigValue("ContentHub:EnsurePublicLinks", "true"), out var ensure) && ensure,
                RequirePublicLinks = bool.TryParse(GetConfigValue("ContentHub:RequirePublicLinks", "true"), out var require) && require,
                PublicLinkActionName = GetConfigValue("ContentHub:PublicLinkActionName", string.Empty),
                PublicLinkActionAssetIdParameterName = GetConfigValue("ContentHub:PublicLinkActionAssetIdParameterName", "assetId")
            };

            if (int.TryParse(GetConfigValue("ContentHub:PublicLinkRetryCount", "3"), out var retryCount))
                options.PublicLinkRetryCount = Math.Max(1, retryCount);

            if (int.TryParse(GetConfigValue("ContentHub:PublicLinkRetryDelayMs", "1500"), out var retryDelay))
                options.PublicLinkRetryDelayMs = Math.Max(250, retryDelay);

            return options;
        }

        internal static void ValidateRequired(ContentHubOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.Endpoint))
                throw new InvalidOperationException("Missing ContentHub:Endpoint in configuration.");
            if (string.IsNullOrWhiteSpace(options.ClientId))
                throw new InvalidOperationException("Missing ContentHub:ClientId in configuration.");
            if (string.IsNullOrWhiteSpace(options.ClientSecret))
                throw new InvalidOperationException("Missing ContentHub:ClientSecret in configuration.");

            var hasUserName = !string.IsNullOrWhiteSpace(options.UserName);
            var hasPassword = !string.IsNullOrWhiteSpace(options.Password);
            if (hasUserName ^ hasPassword)
                throw new InvalidOperationException("Provide both ContentHub:UserName and ContentHub:Password, or leave both empty to use client credentials.");
        }

        private static string GetConfigValue(string key, string defaultValue = "")
        {
            // Load from appsettings using the same Config pattern
            return Config.GetAppSetting(key) ?? defaultValue;
        }
    }
}
