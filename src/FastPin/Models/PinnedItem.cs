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
}
