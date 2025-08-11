using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.DB;
using MinimalApi.Domain.Services;
using MinimalApi.Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Domain.Entity;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Enums;

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

#region Post Create Account

app.MapPost("/admin", ([FromBody] AdminDTO adminDTO, IAdminService adminService) =>
{
    var validation = new ValidationError
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(adminDTO.Email))
        validation.Messages.Add("Email n�o pode ser vazio");

    if (string.IsNullOrEmpty(adminDTO.Password))
        validation.Messages.Add("A senha n�o pode ser vazia");

    if (adminDTO.Profile == null)
        validation.Messages.Add("O perfil n�o pode ser vazio");

    if (validation.Messages.Count > 0)
        return Results.BadRequest(validation);

    var admin = new Admin
    {
        Email = adminDTO.Email,
        Password = adminDTO.Password,
        Profile = adminDTO.Profile.ToString() ?? Profile.Editor.ToString()
    };

    adminService.AddAdmin(admin);

    return Results.Created($"/admin/{admin.Id}", new AdminModelView
    {
        Id = admin.Id,
        Email = admin.Email,
        Profile = admin.Profile
    });
}).WithTags("Login");

#endregion

#region Get All Admins

app.MapGet("/admin/login", ([FromQuery] int? page, IAdminService adminService) =>
{
    var admins = new List<AdminModelView>();
    var admin = adminService.GetAllAdmins(page);
    foreach (var adm in admin)
    {
        admins.Add(new AdminModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Profile = adm.Profile
        });
    }

    return Results.Ok(admins);
}).WithTags("Login");

#endregion

#region Get Admin By Id

app.MapGet("/admin/{id}", ([FromRoute] int id, IAdminService adminService) =>
{
    var admin = adminService.GetAdminById(id);

    if (admin == null)
        return Results.NotFound();

    return Results.Ok(new AdminModelView
    {
        Id = admin.Id,
        Email = admin.Email,
        Profile = admin.Profile
    });
}).WithTags("Login");

#endregion


ValidationError ValidationDTO(VehicleDTO vehicleDTO)
{
    var validationError = new ValidationError
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(vehicleDTO.Name))
        validationError.Messages.Add("O nome n�o pode ser vazio");

    if (string.IsNullOrEmpty(vehicleDTO.Brand))
        validationError.Messages.Add("A marca n�o pode ser vazia");

    if (vehicleDTO.Year < 1885)
        validationError.Messages.Add("Ve�culo muito antigo, aceito apenas anos superiores a 1884");

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
}).WithTags("Ve�culo");

#endregion

#region Get All Vehicles

app.MapGet("/vehicles", ([FromQuery] int? page, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetAllVehicles(page);

    return Results.Ok(vehicles);
}).WithTags("Ve�culo");

#endregion

#region Get Vehicle By Id

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetVehicleById(id);

    if (vehicle == null)
        return Results.NotFound();

    return Results.Ok(vehicle);
}).WithTags("Ve�culo");

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
}).WithTags("Ve�culo");

#endregion

#region Delete Vehicle

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetVehicleById(id);

    if (vehicle == null)
        return Results.NotFound();

    vehicleService.DeleteVehicle(vehicle);

    return Results.NoContent();
}).WithTags("Ve�culo");

#endregion


app.Run();
