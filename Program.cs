using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.DB;
using MinimalApi.Domain.Services;
using MinimalApi.Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Domain.Entity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddDbContext<VehiclesContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

# region Post Login

app.MapPost("/admin/login", ([FromBody] LoginDTO loginDto, IAdminService adminService) =>
{
    if (adminService.Login(loginDto) != null)
        return Results.Ok("Login com Sucesso");
    else
        return Results.Unauthorized();
    
});

#endregion

#region Post Create Vehicle

app.MapPost("/vehicles", ([FromBody] VehicleDTO vehiclesDTO, IVehicleService vehicleService) =>
{
    var vehicle = new Vehicle
    {
        Name = vehiclesDTO.Name,
        Brand = vehiclesDTO.Brand,
        Year = vehiclesDTO.Year
    };

    vehicleService.AddVehicle(vehicle);

    return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
});

#endregion

#region Get All Vehicles

app.MapGet("/vehicles", ([FromQuery] int? page, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetAllVehicles(page);

    return Results.Ok(vehicles);
});

#endregion

#region Get Vehicle By Id

app.MapGet("/vehicles/{id}", ([FromQuery] int id, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetVehicleById(id);

    return Results.Ok(vehicles);
});

#endregion


app.Run();
