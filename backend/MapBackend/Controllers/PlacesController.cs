using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly AppDbContext _context;

    private readonly ILogger<PlacesController> _logger;
    public PlacesController(AppDbContext context, ILogger<PlacesController> logger)
    {
      _context = context;
      _logger = logger;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Place>>> GetPlaces()
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User id not found in token");
        }
        int userId = int.Parse(userIdClaim.Value);
        var places = await _context.Places.Where(p => p.UserId == userId).ToListAsync();
        
        return Ok(places);
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<Place>> GetPlace(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User id not found in token");
        }
        int userId = int.Parse(userIdClaim.Value);
        var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if(place == null)
        {
          return NotFound("Place not found or does not belong to this user.");
        }
        return Ok(place);
    }

    [HttpPost]
    public async Task<ActionResult<Place>> AddPlace([FromBody] Place newPlace)
    {
        if(!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }
        try
        {
          var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
          newPlace.UserId = userId;
          _context.Places.Add(newPlace);
          await _context.SaveChangesAsync();
          return CreatedAtAction(nameof(GetPlace), new { id = newPlace.Id }, newPlace);
        }
        catch(DbUpdateException ex)
        {
          _logger.LogError(ex, "Error adding place to database.");
          return StatusCode(500, "An error occurred while saving the place.");
        }

                    
    }

    
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePlace(int id, [FromBody] Place updatedPlace)
    {
      if (id != updatedPlace.Id )
      {
        return BadRequest("ID in URl and in body of the request must match.");
      }

      if(!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if(userIdClaim == null)
        {
          return Unauthorized("User id not found in token");
        }

        int userId = int.Parse(userIdClaim.Value);
        var existingPlace = await _context.Places.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        
        

      try
      {
        existingPlace.Name = updatedPlace.Name;
        existingPlace.Description = updatedPlace.Description;
        existingPlace.Latitude = updatedPlace.Latitude;
        existingPlace.Longitude = updatedPlace.Longitude;

        await _context.SaveChangesAsync();
        return NoContent();
      }
      catch(DbUpdateException ex)
      {
        _logger.LogError(ex, "Error updating place to database");
        return StatusCode(500, $"Database error: {ex.Message}");
      }
      
    }

    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePlace(int id)
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if(userIdClaim == null)
      {
        return Unauthorized("User id not found in token.");
      }

      int userId = int.Parse(userIdClaim.Value);
      var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

      if (place == null)
        {
            return NotFound("Place not found or does not belong to this user.");
        }

      try
      {
        _context.Places.Remove(place);
        await _context.SaveChangesAsync();
        return NoContent();
      }
      catch(DbUpdateException ex)
      {
        _logger.LogError(ex, "Error deleting place to database");
        return StatusCode(500, "An error occurred while deleting the place.");
      }
        
    }

    
}

public class Place
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage ="Name must be between 3 and 100 characters.", MinimumLength = 3) ]
    public string Name { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.") ]
    public string Description { get; set; }

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be -180 and 180.")]
    public double Longitude { get; set; }

    public int UserId{get; set;}
}
