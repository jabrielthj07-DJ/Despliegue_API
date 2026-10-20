using Capa_Entidades;
using Capa_Datos;
using Capa_Negocio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Conexión a la base de datos
builder.Services.AddDbContext<MiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyección de dependencias
builder.Services.AddScoped<ProductoDatos>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new ProductoDatos(configuration);
});

// Inyección de servicios de capa de negocio (todos en AddScoped)
builder.Services.AddScoped<ProductoNegocio>();
builder.Services.AddScoped<UsuarioNegocio>();
builder.Services.AddScoped<Venta_Negocio>();
builder.Services.AddScoped<Compra_Negocio>();
builder.Services.AddScoped<Rol_Negocio>();
builder.Services.AddScoped<Capa_Negocio.CategoriaNegocio>();
builder.Services.AddScoped<InventarioNegocio>();
builder.Services.AddScoped<ProveedorNegocio>();

// Configuración segura de JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSecretaSuperSeguraVeterinariaML2026!Key";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "miApp",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "miAppUsuarios",
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                mensaje = "Acceso no autorizado. Por favor, inicie sesión y proporcione un token válido (Bearer)."
            });

            return context.Response.WriteAsync(result);
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                mensaje = "Acceso denegado. Solo los administradores tienen permiso para este recurso."
            });

            return context.Response.WriteAsync(result);
        }
    };
});

// Servicios de API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    { 
        Title = "Veterinaria M&L", 
        Version = "v1",
        Contact = new OpenApiContact 
        {
            Name = "Soporte Tecnico",
            Email = "axslpez@gmail.com"
        }
    });

    c.OrderActionsBy(apiDesc =>
    {
        var controller = (apiDesc.ActionDescriptor as ControllerActionDescriptor)?.ControllerName ?? "";

        if (controller == "Login") return "1_Login";
        if (controller == "Rol") return "2_Rol";
        if (controller == "UsuarioRol") return "3_UsuarioRol";
        if (controller == "Categorias") return "4_Categorias";
        if (controller == "Producto") return "5_Producto";
        if (controller == "Ventas") return "6_Ventas";
        if (controller == "Inventario") return "7_Inventario";
        if (controller == "Compras") return "8_Compras";
        if (controller == "Proveedor") return "9_Proveedor";
        return "99_" + controller;
    });

    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT con el formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Carga segura del XML (Evita crash 500.30 si el XML no se incluyó en la publicación)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.EnableAnnotations();
});

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("NuevaPolitica");

// Habilitar Swagger siempre en Azure / Producción
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Veterinaria M&L v1");
    c.RoutePrefix = string.Empty; // Carga Swagger directamente al abrir la URL principal
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
