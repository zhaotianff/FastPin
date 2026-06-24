using System.ComponentModel.DataAnnotations;

namespace FastPin.Api.Models;

public class PinnedItem
{
    [Key]
    public int Id { get; set; }
    public ItemType Type { get; set; }
    public string? TextContent { get; set; }
    public string? RichTextContent { get; set; }
    public byte[]? ImageData { get; set; }
    public string? FilePath { get; set; }
    public byte[]? CachedFileData { get; set; }
    public string? FileName { get; set; }
    public bool IsCached { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public virtual ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    public int? ImageWidth { get; set; }
    public int? ImageHeight { get; set; }
    public long? FileSize { get; set; }
    public ItemSource Source { get; set; } = ItemSource.Clipboard;
    public string? SourceApplication { get; set; }
}

public enum ItemType
{
    Text = 0,
    Image = 1,
    File = 2
}

public enum ItemSource
{
    Clipboard = 0,
    Manual = 1,
    Unknown = 2
}
