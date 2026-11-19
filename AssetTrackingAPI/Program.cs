using AssetTrackingAPI.Context;
using AssetTrackingAPI.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Kestrel para escutar em todas as interfaces (Docker)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "AssetTrackingApi-Development-Key-Change-In-Production";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AssetTrackingAPI";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("cors_policy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Add services to the container
builder.Services.AddDbContext<AssetContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("psql")));

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger/OpenAPI (NSwag)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(options =>
{
    options.DocumentName = "v1";
    options.Title = "Asset Tracking API";
    options.Version = "v1";
    options.Description = "API para gerenciar ativos";
});

var app = builder.Build();

// Habilitar Swagger para todos os ambientes
app.UseOpenApi();
app.UseSwaggerUi();

app.UseCors("cors_policy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Tenta criar o esquema sem impedir a abertura do Swagger quando o banco estiver indisponível.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AssetContext>();
        dbContext.Database.EnsureCreated();
    }
    catch (Exception exception)
    {
        app.Logger.LogWarning(exception,
            "PostgreSQL indisponível. Configure ConnectionStrings:psql ou execute docker-compose antes de usar os endpoints.");
    }
}

// Redireciona a raiz para o Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.MapGet("/health", () => Results.Ok(new
{
    status = "running",
    database = "Configure ConnectionStrings:psql"
}));

app.Run();
