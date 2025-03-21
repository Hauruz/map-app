using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        if(!ModelState.IsValid){
        return BadRequest(ModelState);
      }

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

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePlace(int id, [FromBody] Place updatedPlace)
    {
      if (id != updatedPlace.Id )
      {
        return BadRequest("ID in URl and in body of the request must match.");
      }

      if(!ModelState.IsValid){
        return BadRequest(ModelState);
      }
      var existingPlace = await _context.Places.FindAsync(id);
      if (existingPlace == null)
      {
        return NotFound();
      }

      existingPlace.Name = updatedPlace.Name;
      existingPlace.Description = updatedPlace.Description;
      existingPlace.Latitude = updatedPlace.Latitude;
      existingPlace.Longitude = updatedPlace.Longitude;

      await _context.SaveChangesAsync();
      return NoContent();
    }
}

public class Place
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage ="Name must be between 3 and 100 characters.", MinimumLength = 3) ]
    public string Name { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(300, ErrorMessage ="Description cannot exceed 300 characters.") ]
    public string Description { get; set; }

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage ="Longitude must be -180 and 180.")]
    public double Longitude { get; set; }
}
