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

// DbContext
builder.Services.AddDbContext<AssetContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("psql")));

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// NSwag
builder.Services.AddOpenApiDocument(options =>
{
    options.DocumentName = "v1";
    options.Title = "Asset Tracking API - NSwag";
    options.Version = "v1";
    options.Description = "API para gerenciar ativos (NSwag)";
});

var app = builder.Build();

// Swashbuckle
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Asset Tracking API - Swashbuckle v1");
    c.RoutePrefix = "swagger-sw"; // Acesso: /swagger-sw
});

// NSwag
app.UseOpenApi();
app.UseSwaggerUi(c =>
{
    c.Path = "/swagger-ns"; // Acesso: /swagger-ns
});

app.UseCors("cors_policy");

// Desabilitado HTTPS para teste externo
// app.UseHttpsRedirection();

app.MapControllers();

// Aplica migrações pendentes
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AssetContext>();
    dbContext.Database.Migrate();
}

// Redireciona a raiz para o NSwag Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger-ns"))
   .ExcludeFromDescription();

app.Run();
