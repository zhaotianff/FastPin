using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FastPin.Models
{
    /// <summary>
    /// Represents a pinned item in the FastPin application
    /// </summary>
    public class PinnedItem
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Type of the pinned item: Text, Image, or File
        /// </summary>
        public ItemType Type { get; set; }

        /// <summary>
        /// Text content for text items
        /// </summary>
        public string? TextContent { get; set; }

        /// <summary>
        /// Image data (base64 or binary) for image items
        /// </summary>
        public byte[]? ImageData { get; set; }

        /// <summary>
        /// File path for file items
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Cached file data when cache option is enabled
        /// </summary>
        public byte[]? CachedFileData { get; set; }

        /// <summary>
        /// Original file name
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Indicates whether file should be cached (true) or linked (false)
        /// </summary>
        public bool IsCached { get; set; }

        /// <summary>
        /// When the item was pinned
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// When the item was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Tags associated with this item
        /// </summary>
        public virtual ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();

        /// <summary>
        /// Width of the image (for image items)
        /// </summary>
        public int? ImageWidth { get; set; }

        /// <summary>
        /// Height of the image (for image items)
        /// </summary>
        public int? ImageHeight { get; set; }

        /// <summary>
        /// File size in bytes (for image and file items)
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Source of the pinned item (e.g., Clipboard, Manual)
        /// </summary>
        public ItemSource Source { get; set; } = ItemSource.Clipboard;
        
        /// <summary>
        /// Application or window that was the source of the clipboard content
        /// </summary>
        public string? SourceApplication { get; set; }
    }

    /// <summary>
    /// Enum for different types of pinned items
    /// </summary>
    public enum ItemType
    {
        Text = 0,
        Image = 1,
        File = 2
    }

    /// <summary>
    /// Enum for the source of pinned items
    /// </summary>
    public enum ItemSource
    {
        Clipboard = 0,
        Manual = 1,
        Unknown = 2
    }
}
