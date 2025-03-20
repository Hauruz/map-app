using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly AppDbContext _context;
    public PlacesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Place>>> GetPlaces()
    {
        return Ok(await _context.Places.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Place>> GetPlace(int id)
    {
        var place = await _context.Places.FindAsync(id);
        if (place == null)
        {
            return NotFound();
        }
        return Ok(place);
    }

    [HttpPost]
    public async Task<ActionResult<Place>> AddPlace([FromBody] Place newPlace)
    {
        newPlace.Id = 0; 
        _context.Places.Add(newPlace);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlace), new { id = newPlace.Id }, newPlace);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePlace(int id)
    {
        var place = await _context.Places.FindAsync(id);
        if (place == null)
        {
            return NotFound();
        }

        _context.Places.Remove(place);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class Place
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
