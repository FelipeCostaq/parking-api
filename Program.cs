var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello, World!");

app.MapPost("/login", (LoginDTO loginDto) =>
{
    if (loginDto.Email == "administrador@admin.com" && loginDto.Password == "admin123")
        return Results.Ok("Login com Sucesso");
    else
        return Results.Unauthorized();
    
});

app.Run();

public class LoginDTO
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
