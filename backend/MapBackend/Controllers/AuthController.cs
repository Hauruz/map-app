using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;


[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
  private readonly AppDbContext _context;
  private readonly IConfiguration _config;

  public AuthController(AppDbContext context, IConfiguration config)
  {
    _context = context;
    _config = config;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] User user) 
  {
    if (await _context.Users.AnyAsync(u=> u.Email == user.Email))
    {
      return BadRequest("User already exists.");
    }
    
      user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
      _context.Users.Add(user);
      await _context.SaveChangesAsync();
      return Ok("User registered sucesfully.");
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] User user)
  {
    var existingUser = await _context.Users.FirstOrDefaultAsync(u=> u.Email == user.Email);
    if(existingUser == null || !BCrypt.Net.BCrypt.Verify(user.PasswordHash, existingUser.PasswordHash))
    {
      return Unauthorized("Invalid credentials.");
    }
    var token = GenerateJwtToken(existingUser);
    return Ok(new{Token = token});
  }

  private string GenerateJwtToken(User user)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
    };

    var token = new JwtSecurityToken(
      _config["Jwt:Issuer"],
      _config["Jwt:Audience"],
      claims,
      expires: DateTime.UtcNow.AddHours(2),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}