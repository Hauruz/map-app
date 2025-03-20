using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
  private static readonly List<Place> places = new()
  {
    new Place {Id = 1, Name = "Эйфелевая башня", Description = "Знаменитая достопримечательность Парижа", Latitude = 48.8584, Longitude = 2.2945},
    new Place {Id = 2, Name = "Колизей", Description = "Древний римский амфитеатр", Latitude = 41.8902, Longitude = 12.4922}
  };

  [HttpGet]
  public ActionResult<IEnumerable<Place>> GetPlaces()
  {
    return Ok(places);
  }

  [HttpGet("{id}")]
  public ActionResult<Place> GetPlace(int id)
  {
    var place = places.FirstOrDefault(p => p.Id == id);
    if (place == null)
    {
      return NotFound();
    }
    return Ok(place);
  }

  [HttpPost]
  public ActionResult<Place> AddPlace([FromBody] Place newPlace)
  {
    newPlace.Id = places.Max(p => p.Id) + 1;
    places.Add(newPlace);
    return CreatedAtAction(nameof(GetPlace), new {id = newPlace.Id}, newPlace);
  }

  [HttpDelete("{id}")]
  public ActionResult DeletePlace(int id)
  {
    var place = places.FirstOrDefault(p => p.Id == id);
    if (place ==null)
    {
      return NotFound();
    }
    places.Remove(place);
    return NoContent();
  }
}

public class Place
{
  public int Id{get; set;}
  public string Name{get; set;}
  public string Description{get; set;}
  public double Latitude{get; set;}
  public double Longitude{get; set;}
}
