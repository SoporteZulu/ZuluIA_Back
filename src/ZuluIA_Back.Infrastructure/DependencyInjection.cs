using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Infrastructure.Persistence;
using ZuluIA_Back.Infrastructure.Persistence.Repositories;
using ZuluIA_Back.Infrastructure.Services;

namespace ZuluIA_Back.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string no configurada.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(60);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ITerceroRepository, TerceroRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IAsientoRepository, AsientoRepository>();

        services.AddScoped(typeof(IRepository<Cobro>), typeof(BaseRepository<Cobro>));
        services.AddScoped(typeof(IRepository<CobroMedio>), typeof(BaseRepository<CobroMedio>));
        services.AddScoped(typeof(IRepository<Pago>), typeof(BaseRepository<Pago>));
        services.AddScoped(typeof(IRepository<PagoMedio>), typeof(BaseRepository<PagoMedio>));
        services.AddScoped(typeof(IRepository<AsientoLinea>), typeof(BaseRepository<AsientoLinea>));
        services.AddScoped(typeof(IRepository<ComprobanteItem>), typeof(BaseRepository<ComprobanteItem>));

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICurrentUserService, HttpCurrentUserService>();

        return services;
    }
}