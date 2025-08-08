using Microsoft.EntityFrameworkCore;
using MinimalApi.Infrastructure.DB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VehiclesContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();

app.MapGet("/", () => "Hello, World!");

app.MapPost("/login", (MinimalApi.Domain.DTO.LoginDTO loginDto) =>
{
    if (loginDto.Email == "administrador@admin.com" && loginDto.Password == "admin123")
        return Results.Ok("Login com Sucesso");
    else
        return Results.Unauthorized();
    
});

app.Run();
