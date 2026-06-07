using System.Collections.Generic;

namespace SitecoreCommander.Bynder.Model;

/// <summary>Represents a Bynder asset with complete metadata and URLs.</summary>
public class BynderAsset
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }
    public required string DownloadUrl { get; set; }
    public required string[] Tags { get; set; }

    public string? ThumbnailMiniUrl { get; set; }
    public string? ThumbnailWebimageUrl { get; set; }
    public string? ThumbnailSmallUrl { get; set; }
    public string? OriginalUrl { get; set; }
    public string? PublicUrl { get; set; }
    public string? Copyright { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
    public long? FileSize { get; set; }
    public string? DateCreated { get; set; }
    public string? DateModified { get; set; }
    public Dictionary<string, string>? Thumbnails { get; set; }
    public Dictionary<string, object>? RawJson { get; set; }
}

/// <summary>Paginated result set of Bynder assets.</summary>
public class BynderAssetResultSet
{
    public List<BynderAsset> Assets { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
        /// <summary>Total assets in Bynder as reported by X-Total-Count response header. Null if not provided by API.</summary>
        public int? ApiTotalCount { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>Represents a media mapping between Sitecore item and Bynder asset for JSS migration.</summary>
public class BynderMediaMapping
{
    public required string SourceMediaGuid { get; set; }
    public required string SourceMediaPath { get; set; }
    public required string BynderAssetId { get; set; }
    public string? BynderAssetName { get; set; }
    public string Status { get; set; } = "pending";
    public string? ContentHubReference { get; set; }
    public DateTime MappedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
    public string? Notes { get; set; }
}
