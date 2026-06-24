using FastPin.Api.Contracts;
using FastPin.Api.Data;
using FastPin.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastPin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PinnedItemsController : ControllerBase
{
    private readonly FastPinApiDbContext _dbContext;

    public PinnedItemsController(FastPinApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<PinnedItem>>> GetAll()
    {
        var items = await _dbContext.PinnedItems
            .Include(p => p.ItemTags)
            .ThenInclude(it => it.Tag)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<PinnedItem>> Create([FromBody] PinnedItem item)
    {
        item.Id = 0;
        item.ModifiedDate = DateTime.Now;

        _dbContext.PinnedItems.Add(item);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PinnedItem>> GetById(int id)
    {
        var item = await _dbContext.PinnedItems
            .Include(p => p.ItemTags)
            .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _dbContext.PinnedItems.FirstOrDefaultAsync(p => p.Id == id);
        if (item == null)
        {
            return NotFound();
        }

        _dbContext.PinnedItems.Remove(item);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/cache")]
    public async Task<IActionResult> UpdateFileCache(int id, [FromBody] FileCacheUpdateRequest request)
    {
        var item = await _dbContext.PinnedItems.FirstOrDefaultAsync(p => p.Id == id);
        if (item == null)
        {
            return NotFound();
        }

        item.IsCached = request.IsCached;
        item.CachedFileData = request.IsCached ? request.CachedFileData : null;
        item.ModifiedDate = DateTime.Now;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{itemId:int}/tags/{tagId:int}")]
    public async Task<IActionResult> AddTagToItem(int itemId, int tagId)
    {
        var itemExists = await _dbContext.PinnedItems.AnyAsync(p => p.Id == itemId);
        var tagExists = await _dbContext.Tags.AnyAsync(t => t.Id == tagId);
        if (!itemExists || !tagExists)
        {
            return NotFound();
        }

        var relationExists = await _dbContext.ItemTags.AnyAsync(it => it.PinnedItemId == itemId && it.TagId == tagId);
        if (!relationExists)
        {
            _dbContext.ItemTags.Add(new ItemTag { PinnedItemId = itemId, TagId = tagId });
            await _dbContext.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{itemId:int}/tags/{tagId:int}")]
    public async Task<IActionResult> RemoveTagFromItem(int itemId, int tagId)
    {
        var relation = await _dbContext.ItemTags.FirstOrDefaultAsync(it => it.PinnedItemId == itemId && it.TagId == tagId);
        if (relation == null)
        {
            return NotFound();
        }

        _dbContext.ItemTags.Remove(relation);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
