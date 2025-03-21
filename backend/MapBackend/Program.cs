using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var connectionString = "Data Source=places.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();
    

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy=>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});




var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();


app.Run();