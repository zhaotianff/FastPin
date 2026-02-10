using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FastPin.Models
{
    /// <summary>
    /// Represents a tag that can be applied to pinned items
    /// </summary>
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the tag
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Color for the tag (hex format)
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Class/Category for the tag (e.g., "Work", "Personal", "Project")
        /// </summary>
        public string? Class { get; set; }

        /// <summary>
        /// Items associated with this tag
        /// </summary>
        public virtual ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    }
}
