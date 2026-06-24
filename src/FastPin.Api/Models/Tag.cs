using System.ComponentModel.DataAnnotations;

namespace FastPin.Api.Models;

public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Color { get; set; }
    public string? Class { get; set; }
    public virtual ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
}
