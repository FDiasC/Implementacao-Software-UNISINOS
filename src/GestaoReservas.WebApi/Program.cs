using System.Text.Json.Serialization;
using GestaoReservas.WebApi.Middleware;
using GestaoReservas.WebApi.Handlers;
using GestaoReservas.Domain.Providers;
using GestaoReservas.Infrastructure.Data;
using GestaoReservas.Infrastructure.Providers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUsuarioProvider, UsuarioProvider>();
builder.Services.AddScoped<ICategoriaProvider, CategoriaProvider>();
builder.Services.AddScoped<ILocalProvider, LocalProvider>();

builder.Services.AddScoped<IUsuarioHandler, UsuarioHandler>();
builder.Services.AddScoped<ICategoriaHandler, CategoriaHandler>();
builder.Services.AddScoped<ILocalHandler, LocalHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Gestão de Reservas v1"));
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
