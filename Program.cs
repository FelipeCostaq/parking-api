using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.DB;
using MinimalApi.Domain.Services;
using MinimalApi.Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Domain.Entity;
using MinimalApi.Domain.ModelViews;

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
    
}).WithTags("Login");

#endregion

ValidationError ValidationDTO(VehicleDTO vehicleDTO)
{
    var validationError = new ValidationError
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(vehicleDTO.Name))
        validationError.Messages.Add("O nome não pode ser vazio");

    if (string.IsNullOrEmpty(vehicleDTO.Brand))
        validationError.Messages.Add("A marca não pode ser vazia");

    if (vehicleDTO.Year < 1885)
        validationError.Messages.Add("Veículo muito antigo, aceito apenas anos superiores a 1884");

    return validationError;
}

#region Post Create Vehicle

app.MapPost("/vehicles", ([FromBody] VehicleDTO vehiclesDTO, IVehicleService vehicleService) =>
{
    var validation = ValidationDTO(vehiclesDTO);
    if (validation.Messages.Count > 0)
        return Results.BadRequest(validation);

    var vehicle = new Vehicle
    {
        Name = vehiclesDTO.Name,
        Brand = vehiclesDTO.Brand,
        Year = vehiclesDTO.Year
    };

    vehicleService.AddVehicle(vehicle);

    return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
}).WithTags("Veículo");

#endregion

#region Get All Vehicles

app.MapGet("/vehicles", ([FromQuery] int? page, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetAllVehicles(page);

    return Results.Ok(vehicles);
}).WithTags("Veículo");

#endregion

#region Get Vehicle By Id

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetVehicleById(id);

    if (vehicle == null)
        return Results.NotFound();

    return Results.Ok(vehicle);
}).WithTags("Veículo");

#endregion

#region Put Edit Vehicle

app.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetVehicleById(id);
    if (vehicle == null)
        return Results.NotFound();

    var validation = ValidationDTO(vehicleDTO);

    if (validation.Messages.Count > 0)
        return Results.BadRequest(validation);

    vehicle.Name = vehicleDTO.Name;
    vehicle.Brand = vehicleDTO.Brand;
    vehicle.Year = vehicleDTO.Year;

    vehicleService.EditVehicle(vehicle);

    return Results.Ok(vehicle);
}).WithTags("Veículo");

#endregion

#region Delete Vehicle

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetVehicleById(id);

    if (vehicle == null)
        return Results.NotFound();

    vehicleService.DeleteVehicle(vehicle);

    return Results.NoContent();
}).WithTags("Veículo");

#endregion


app.Run();
