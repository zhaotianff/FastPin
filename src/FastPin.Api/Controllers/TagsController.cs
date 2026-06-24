using FastPin.Api.Contracts;
using FastPin.Api.Data;
using FastPin.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastPin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly FastPinApiDbContext _dbContext;

    public TagsController(FastPinApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<Tag>>> GetAll()
    {
        return Ok(await _dbContext.Tags.OrderBy(t => t.Name).ToListAsync());
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<Tag>> GetByName(string name)
    {
        var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == name);
        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<Tag>> Create([FromBody] Tag tag)
    {
        tag.Id = 0;
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
    }

    [HttpPost("ensure")]
    public async Task<ActionResult<Tag>> Ensure([FromBody] TagEnsureRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest();
        }

        var existing = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == name);
        if (existing != null)
        {
            return Ok(existing);
        }

        var newTag = new Tag { Name = name };
        _dbContext.Tags.Add(newTag);
        await _dbContext.SaveChangesAsync();

        return Ok(newTag);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Tag>> GetById(int id)
    {
        var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Tag update)
    {
        var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null)
        {
            return NotFound();
        }

        tag.Name = update.Name;
        tag.Class = update.Class;
        tag.Color = update.Color;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _dbContext.Tags
            .Include(t => t.ItemTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null)
        {
            return NotFound();
        }

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
