using System.Text;
using HorusEye.Api.Hubs;
using HorusEye.Api.Middleware;
using HorusEye.Api.Services;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File(
        path: "logs/log-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        formatter: new Serilog.Formatting.Json.JsonFormatter())
    .Enrich.WithProperty("Application", "HorusEye")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HorusEyeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<HorusEyeDbContext>()
.AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException(
    "Jwt:Key no configurada. En desarrollo usar appsettings.Development.json, en producción variable Jwt__Key");
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloLectura", policy =>
        policy.RequireRole("Usuario de Consulta"));
    options.AddPolicy("GestionTotal", policy =>
        policy.RequireRole("Usuario de Gestión"));
});

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PermisoService>();

var allowedOrigin = builder.Configuration["CORS__AllowedOrigin"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (!string.IsNullOrEmpty(allowedOrigin))
        {
            policy.WithOrigins(allowedOrigin)
                .AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => true);
        }

        policy.AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HorusEyeDbContext>();

    try
    {
        await context.Database.MigrateAsync();
    }
    catch
    {
        context.Database.EnsureCreated();
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") " +
            "VALUES ('20260529030543_InitialCreate', '10.0.0')");
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[]
    {
        "Administrador del Sistema",
        "Asistente del Administrador del Sistema",
        "Soporte del Sistema",
        "Administrador del Proveedor",
        "Asistente del Proveedor",
        "Administrador del Cliente",
        "Asistente del Cliente"
    };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var adminEmail = "admin@horuseye.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = "Administrador",
            Email = adminEmail,
            EmailConfirmed = true
        };
        var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrador del Sistema");
            context.UsuariosExtendidos.Add(new HorusEye.Core.Entities.UsuarioExtendido
            {
                Id = adminUser.Id
            });
            await context.SaveChangesAsync();
            Log.Information("Usuario administrador creado: {Email}", adminEmail);
        }
    }
    else
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser != null)
        {
            var currentRoles = await userManager.GetRolesAsync(adminUser);
            if (currentRoles.Contains("Usuario de Gestión"))
            {
                await userManager.RemoveFromRoleAsync(adminUser, "Usuario de Gestión");
                await userManager.AddToRoleAsync(adminUser, "Administrador del Sistema");
                Log.Information("Rol de admin migrado de 'Usuario de Gestión' a 'Administrador del Sistema'");
            }
        }
    }

    var consultaEmail = "consulta@horuseye.com";
    if (await userManager.FindByEmailAsync(consultaEmail) == null)
    {
        var consultaUser = new IdentityUser
        {
            UserName = "Consulta",
            Email = consultaEmail,
            EmailConfirmed = true
        };
        var createResult = await userManager.CreateAsync(consultaUser, "Consulta123!");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(consultaUser, "Asistente del Cliente");
            context.UsuariosExtendidos.Add(new HorusEye.Core.Entities.UsuarioExtendido
            {
                Id = consultaUser.Id
            });
            await context.SaveChangesAsync();
            Log.Information("Usuario de consulta creado: {Email}", consultaEmail);
        }
    }
    else
    {
        var consultaUser = await userManager.FindByEmailAsync(consultaEmail);
        if (consultaUser != null)
        {
            var currentRoles = await userManager.GetRolesAsync(consultaUser);
            if (currentRoles.Contains("Usuario de Consulta"))
            {
                await userManager.RemoveFromRoleAsync(consultaUser, "Usuario de Consulta");
                await userManager.AddToRoleAsync(consultaUser, "Asistente del Cliente");
                Log.Information("Rol de consulta migrado de 'Usuario de Consulta' a 'Asistente del Cliente'");
            }
        }
    }

    if (!await roleManager.RoleExistsAsync("Usuario de Consulta"))
    {
        Log.Information("Roles antiguos eliminados, sistema migrado a 7 roles nuevos");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MovimientosHub>("/hubs/movimientos");

try
{
    Log.Information("Iniciando HorusEye API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
