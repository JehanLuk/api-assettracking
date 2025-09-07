using AssetTrackingAPI.Context;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Kestrel para escutar em todas as interfaces (Docker)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

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

// Desabilitado HTTPS para teste externo
// app.UseHttpsRedirection();

app.MapControllers();

// Aplica migrações pendentes do banco
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AssetContext>();
    dbContext.Database.Migrate();
}

// Redireciona a raiz para o Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.Run();
