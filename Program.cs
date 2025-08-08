using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.DB;
using MinimalApi.Domain.Services;
using MinimalApi.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddDbContext<VehiclesContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();

app.MapPost("/login", ([FromBody] LoginDTO loginDto, IAdminService adminService) =>
{
    if (adminService.Login(loginDto) != null)
        return Results.Ok("Login com Sucesso");
    else
        return Results.Unauthorized();
    
});

app.Run();
