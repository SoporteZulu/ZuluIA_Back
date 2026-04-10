using ZuluIA_Back.Api.Middleware;
using ZuluIA_Back.Api.HostedServices;
using ZuluIA_Back.Application;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    DotNetEnv.Env.Load();
    Log.Information("Iniciando ZuluIA Back API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "ZuluIA Back API",
            Version     = "v1",
            Description = "API del sistema de gestión ZuluIA"
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Type         = SecuritySchemeType.Http,
            Scheme       = "Bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Description  = "Ingrese el token JWT de Supabase Auth"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var jwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET")
                 ?? builder.Configuration["Supabase:JwtSecret"]!;

    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                 ?? builder.Configuration["Jwt:Issuer"]!;

    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                   ?? builder.Configuration["Jwt:Audience"]!;

    var requireHttpsMetadata = bool.TryParse(builder.Configuration["JwtSettings:RequireHttpsMetadata"], out var requireHttps)
        ? requireHttps
        : !builder.Environment.IsDevelopment();

    var isDevelopment = builder.Environment.IsDevelopment();

    if (string.IsNullOrWhiteSpace(jwtSecret))
    {
        if (isDevelopment)
        {
            // Usar un secret dummy en desarrollo para que la app arranque
            jwtSecret = "dev-only-dummy-secret-32-chars-min!!";
            Log.Warning("SUPABASE_JWT_SECRET no configurado. Usando valor dummy para desarrollo.");
        }
        else
        {
            throw new InvalidOperationException("SUPABASE_JWT_SECRET no está configurado.");
        }
    }

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = requireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer   = true,
                ValidIssuer      = jwtIssuer,
                ValidateAudience = true,
                ValidAudience    = jwtAudience,
                ValidateLifetime = true,
                ClockSkew        = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();
    builder.Services.AddScoped<ZuluIA_Back.Api.Security.PermissionAuthorizationFilter>();
    builder.Services.AddScoped<ZuluIA_Back.Api.Security.CriticalOperationAuditFilter>();

    var allowedOrigins = (Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
                         ?? string.Join(",", builder.Configuration
                               .GetSection("Cors:AllowedOrigins")
                               .Get<string[]>() ?? []))
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    if (!builder.Environment.IsDevelopment())
    {
        builder.Services.AddHostedService<BatchSchedulerHostedService>();
        builder.Services.AddHostedService<ImpresionSpoolHostedService>();
    }

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
    });

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("swagger/v1/swagger.json", "ZuluIA Back API v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<CurrentUserMiddleware>();
    app.MapControllers();
    app.MapGet("/health", async (IApplicationDbContext db, IWebHostEnvironment env, OperacionesBatchSettingsService batchSettingsService, CancellationToken ct) =>
    {
        var dbOk = await db.Config.AnyAsync(ct);
        var batch = await batchSettingsService.ResolveAsync(ct);

        return Results.Ok(new
        {
            status = "healthy",
            app = "ZuluIA_Back",
            environment = env.EnvironmentName,
            database = dbOk ? "online" : "no-data",
            schedulerEnabled = batch.SchedulerHabilitado,
            spoolEnabled = batch.SpoolHabilitado,
            timestamp = DateTime.UtcNow
        });
    }).AllowAnonymous();

    app.MapGet("/health/detailed", async (IApplicationDbContext db, IWebHostEnvironment env, OperacionesBatchSettingsService batchSettingsService, GoLiveOperativoReadinessService goLiveOperativoReadinessService, FiscalHardwareDiagnosticService fiscalHardwareDiagnosticService, CancellationToken ct) =>
    {
        var ahora = DateTimeOffset.UtcNow;
        var batch = await batchSettingsService.ResolveAsync(ct);
        var readiness = await goLiveOperativoReadinessService.EvaluateAsync(ct);
        var hardware = await fiscalHardwareDiagnosticService.DiagnoseAsync(ct);

        return Results.Ok(new
        {
            status = readiness.ReadyForGoLive ? "healthy" : "degraded",
            app = "ZuluIA_Back",
            environment = env.EnvironmentName,
            database = await db.Config.AnyAsync(ct) ? "online" : "no-data",
            scheduler = new
            {
                batch.SchedulerHabilitado,
                batch.SchedulerPollSeconds,
                ProgramacionesVencidas = await db.BatchProgramaciones.AsNoTracking().CountAsync(x => x.Activa && x.DeletedAt == null && x.ProximaEjecucion <= ahora, ct)
            },
            spool = new
            {
                batch.SpoolHabilitado,
                batch.SpoolPollSeconds,
                Pendientes = await db.ImpresionSpoolTrabajos.AsNoTracking().CountAsync(x => x.DeletedAt == null && x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Pendiente, ct),
                Errores = await db.ImpresionSpoolTrabajos.AsNoTracking().CountAsync(x => x.DeletedAt == null && x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error, ct)
            },
            readiness,
            hardware,
            timestamp = DateTime.UtcNow
        });
    }).AllowAnonymous();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}