using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastPin.Api.Models;

public class ItemTag
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("PinnedItem")]
    public int PinnedItemId { get; set; }
    public virtual PinnedItem PinnedItem { get; set; } = null!;

    [ForeignKey("Tag")]
    public int TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;
}
