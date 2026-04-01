using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ZuluIA_Back.Application.Common.Behaviors;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.Services;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Application.Features.Contratos.Services;
using ZuluIA_Back.Application.Features.Compras.Services;
using ZuluIA_Back.Application.Features.Diagnosticos.Services;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Application.Features.Finanzas.Services;
using ZuluIA_Back.Application.Features.Fiscal.Services;
using ZuluIA_Back.Application.Features.Impresion.Interfaces;
using ZuluIA_Back.Application.Features.Impresion.Services;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Application.Features.ListasPrecios.Services;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Application.Features.Produccion.Services;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Ventas.Services;

namespace ZuluIA_Back.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddScoped<ChequeAuditoriaService>();
        services.AddScoped<ColegioService>();
        services.AddScoped<ContratosService>();
        services.AddScoped<CircuitoComprasService>();
        services.AddScoped<AfipWsfeService>();
        services.AddScoped<CartaPorteWorkflowService>();
        services.AddScoped<FacturacionBatchService>();
        services.AddScoped<DiagnosticoPlanTrabajoService>();
        services.AddScoped<FiscalContabilidadLocalService>();
        services.AddScoped<IImpresoraFiscalAdapter, EpsonImpresoraFiscalAdapter>();
        services.AddScoped<IImpresoraFiscalAdapter, HasarImpresoraFiscalAdapter>();
        services.AddScoped<ImpresionFiscalService>();
        services.AddScoped<ImpresionDocumentosService>();
        services.AddScoped<ExternalIntegrationCertificationService>();
        services.AddScoped<ExternalFiscalPrecheckService>();
        services.AddScoped<ExternalFiscalReadinessService>();
        services.AddScoped<ExternalProviderErrorCatalogService>();
        services.AddScoped<GoLiveOperativoReadinessService>();
        services.AddScoped<ZuluAppReplacementCertificationService>();
        services.AddScoped<FiscalHardwareDiagnosticService>();
        services.AddScoped<ArchivoImportLayoutValidationService>();
        services.AddScoped<ArchivoImportLayoutProfileService>();
        services.AddScoped<ExternalIntegrationProviderSettingsService>();
        services.AddScoped<ExternalProviderHttpGateway>();
        services.AddScoped<ExternalIntegrationResilienceService>();
        services.AddScoped<ArchivoTabularParserService>();
        services.AddScoped<ImportacionArchivoService>();
        services.AddScoped<OperacionesBatchSettingsService>();
        services.AddScoped<BatchSchedulerService>();
        services.AddScoped<ImpresionSpoolService>();
        services.AddScoped<IntegracionProcesoService>();
        services.AddScoped<LegacyExportacionService>();
        services.AddScoped<LogisticaInternaService>();
        services.AddScoped<ItemCommercialStockService>();
        services.AddScoped<PrecioListaResolutionService>();
        services.AddScoped<FormulaProduccionHistorialService>();
        services.AddScoped<PuntoVentaFiscalService>();
        services.AddScoped<ReporteExportacionService>();
        services.AddScoped<ReportesService>();
        services.AddScoped<RrhhService>();
        services.AddScoped<PagoWorkflowService>();
        services.AddScoped<TesoreriaService>();
        services.AddScoped<TerceroOperacionValidationService>();
        services.AddScoped<CircuitoVentasService>();
        services.AddScoped<NotaCreditoValidationService>();
        services.AddScoped<NotaDebitoValidationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}