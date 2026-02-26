using ZuluIA_Back.Api.Middleware;
using ZuluIA_Back.Application;
using ZuluIA_Back.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
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
            Name        = "Authorization",
            Type        = SecuritySchemeType.Http,
            Scheme      = "Bearer",
            BearerFormat = "JWT",
            In          = ParameterLocation.Header,
            Description = "Ingrese el token JWT de Supabase Auth"
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

    var jwtAuthority = Environment.GetEnvironmentVariable("JWT_AUTHORITY")
                    ?? builder.Configuration["Jwt:Authority"]!;

    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                 ?? builder.Configuration["Jwt:Issuer"]!;

    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                   ?? builder.Configuration["Jwt:Audience"]!;

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = jwtAuthority;
            options.Audience  = jwtAudience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer   = true,
                ValidIssuer      = jwtIssuer,
                ValidateAudience = true,
                ValidAudience    = jwtAudience,
                ValidateLifetime = true,
                ClockSkew        = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

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

    var app = builder.Build();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
    });

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZuluIA Back API v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<CurrentUserMiddleware>();
    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy",
        app = "ZuluIA_Back",
        timestamp = DateTime.UtcNow
    })).AllowAnonymous();

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