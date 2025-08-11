using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.DB;
using MinimalApi.Domain.Services;
using MinimalApi.Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Domain.Entity;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();

if (string.IsNullOrEmpty(key)) 
    key = "123456";

builder.Services.AddAuthentication(option =>
{
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddDbContext<VehiclesContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

# region Post Login

app.MapPost("/admin/login", ([FromBody] LoginDTO loginDto, IAdminService adminService) =>
{
    var admin = adminService.Login(loginDto);

    if (adminService.Login(loginDto) != null)
    {
        string token = GenerateToken(admin);
        return Results.Ok(new LoggedAdmin
        {
            Email = admin.Email,
            Profile = admin.Profile,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
    
}).WithTags("Login");

#endregion

#region Token JWT

string GenerateToken(Admin admin)
{
    if (string.IsNullOrEmpty(key))
        return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim(ClaimTypes.Email, admin.Email),
        new Claim("Profile", admin.Profile)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credential
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

#endregion

#region Post Create Account

app.MapPost("/admin", ([FromBody] AdminDTO adminDTO, IAdminService adminService) =>
{
    var validation = new ValidationError
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(adminDTO.Email))
        validation.Messages.Add("Email não pode ser vazio");

    if (string.IsNullOrEmpty(adminDTO.Password))
        validation.Messages.Add("A senha não pode ser vazia");

    if (adminDTO.Profile == null)
        validation.Messages.Add("O perfil não pode ser vazio");

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
}).RequireAuthorization().WithTags("Login");

#endregion

#region Get All Admins

app.MapGet("/admin", ([FromQuery] int? page, IAdminService adminService) =>
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
}).RequireAuthorization().WithTags("Login");

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
}).RequireAuthorization().WithTags("Login");

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
}).RequireAuthorization().WithTags("Veículo");

#endregion

#region Get All Vehicles

app.MapGet("/vehicles", ([FromQuery] int? page, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetAllVehicles(page);

    return Results.Ok(vehicles);
}).RequireAuthorization().WithTags("Veículo");

#endregion

#region Get Vehicle By Id

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetVehicleById(id);

    if (vehicle == null)
        return Results.NotFound();

    return Results.Ok(vehicle);
}).RequireAuthorization().WithTags("Veículo");

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
}).RequireAuthorization().WithTags("Veículo");

#endregion

#region Delete Vehicle

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetVehicleById(id);

    if (vehicle == null)
        return Results.NotFound();

    vehicleService.DeleteVehicle(vehicle);

    return Results.NoContent();
}).RequireAuthorization().WithTags("Veículo");

#endregion


app.Run();
