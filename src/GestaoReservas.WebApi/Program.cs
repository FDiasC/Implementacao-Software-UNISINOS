using System.Text;
using System.Text.Json.Serialization;
using GestaoReservas.WebApi.Common.Auth;
using GestaoReservas.WebApi.Middleware;
using GestaoReservas.WebApi.Handlers;
using GestaoReservas.Domain.Providers;
using GestaoReservas.Infrastructure.Data;
using GestaoReservas.Infrastructure.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

// OpenAPI com o esquema "Bearer", para o botão "Authorize" do Swagger enviar o token.
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token obtido em POST /api/auth/token."
    };
    document.Security ??= [];
    document.Security.Add(new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    return Task.CompletedTask;
}));

var origensPermitidas = builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(origensPermitidas)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Autenticação JWT
var jwtSecao = builder.Configuration.GetSection(JwtOptions.Secao);
builder.Services.Configure<JwtOptions>(jwtSecao);
var jwt = jwtSecao.Get<JwtOptions>() ?? new JwtOptions();
if (Encoding.UTF8.GetByteCount(jwt.Chave) < 32)
{
    throw new InvalidOperationException(
        "Configure 'Jwt:Chave' com pelo menos 32 caracteres (appsettings.Development.json ou User Secrets).");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emissor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name"
        };
        // 401 sem corpo por padrão; devolve ProblemDetails como o restante da API.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                var mensagem = context.AuthenticateFailure is SecurityTokenExpiredException
                    ? "Sessão expirada. Faça login novamente."
                    : "É necessário estar autenticado para acessar este recurso.";
                await ExceptionHandlingMiddleware.EscreverProblemaAsync(context.HttpContext, StatusCodes.Status401Unauthorized, mensagem);
            }
        };
    });

// Todas as rotas exigem token, exceto as marcadas com [AllowAnonymous] (login e cadastro de usuário).
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUsuarioProvider, UsuarioProvider>();
builder.Services.AddScoped<ICategoriaProvider, CategoriaProvider>();
builder.Services.AddScoped<ILocalProvider, LocalProvider>();
builder.Services.AddScoped<IRecursoProvider, RecursoProvider>();
builder.Services.AddScoped<IReservaProvider, ReservaProvider>();

builder.Services.AddScoped<IAuthHandler, AuthHandler>();
builder.Services.AddScoped<IUsuarioHandler, UsuarioHandler>();
builder.Services.AddScoped<ICategoriaHandler, CategoriaHandler>();
builder.Services.AddScoped<ILocalHandler, LocalHandler>();
builder.Services.AddScoped<IRecursoHandler, RecursoHandler>();
builder.Services.AddScoped<IReservaHandler, ReservaHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
        options.EnablePersistAuthorization();
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
