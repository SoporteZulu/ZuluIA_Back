using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Application.Features.Reportes.DTOs;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ZuluAppReplacementCertificationService(
    IApplicationDbContext db,
    GoLiveOperativoReadinessService goLiveOperativoReadinessService,
    ArchivoImportLayoutProfileService layoutProfileService,
    OperacionesBatchSettingsService batchSettingsService)
{
    public async Task<ZuluAppReplacementCertificationDto> EvaluateAsync(CancellationToken ct = default)
    {
        var readiness = await goLiveOperativoReadinessService.EvaluateAsync(ct);
        var batchSettings = await batchSettingsService.ResolveAsync(ct);
        var supportedCircuits = layoutProfileService.GetSupportedCircuits();

        var coreCounts = new Dictionary<string, long>
        {
            ["Sucursales"] = await db.Sucursales.AsNoTracking().LongCountAsync(ct),
            ["Usuarios"] = await db.Usuarios.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["Terceros"] = await db.Terceros.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["Items"] = await db.Items.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["Monedas"] = await db.Monedas.AsNoTracking().LongCountAsync(ct),
            ["TiposComprobante"] = await db.TiposComprobante.AsNoTracking().LongCountAsync(ct),
            ["Comprobantes"] = await db.Comprobantes.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["Cobros"] = await db.Cobros.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["Pagos"] = await db.Pagos.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["Contratos"] = await db.Contratos.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["Cedulones"] = await db.Cedulones.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["OrdenesPreparacion"] = await db.OrdenesPreparacion.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["TransferenciasDeposito"] = await db.TransferenciasDeposito.AsNoTracking().LongCountAsync(x => !x.IsDeleted, ct),
            ["Empleados"] = await db.Empleados.AsNoTracking().LongCountAsync(ct),
            ["Permisos"] = await db.Seguridad.AsNoTracking().LongCountAsync(ct),
            ["PermisosUsuario"] = await db.SeguridadUsuario.AsNoTracking().LongCountAsync(x => x.Valor, ct),
            ["MenusUsuario"] = await db.MenuUsuario.AsNoTracking().LongCountAsync(ct),
            ["ParametrosUsuario"] = await db.ParametrosUsuario.AsNoTracking().LongCountAsync(ct)
        };

        var smokeReady = coreCounts["Sucursales"] > 0
            && coreCounts["Usuarios"] > 0
            && coreCounts["Terceros"] > 0
            && coreCounts["Items"] > 0
            && coreCounts["Monedas"] > 0
            && coreCounts["TiposComprobante"] > 0;

        var legacyCompatibilityReady = supportedCircuits.Count > 0 && !string.IsNullOrWhiteSpace(batchSettings.LayoutLegacyProfile);
        var securityBaselineReady = coreCounts["Permisos"] > 0 && coreCounts["PermisosUsuario"] > 0 && coreCounts["MenusUsuario"] > 0;
        var operationalGoLiveReady = readiness.ReadyForGoLive;
        var replacementReady = smokeReady && legacyCompatibilityReady && securityBaselineReady && operationalGoLiveReady;
        var moduleSmoke = BuildModuleSmoke(coreCounts, smokeReady, securityBaselineReady, operationalGoLiveReady);
        var moduleParity = BuildModuleParity(coreCounts, legacyCompatibilityReady, securityBaselineReady, operationalGoLiveReady);
        var evidenceChecklist = BuildEvidenceChecklist(smokeReady, legacyCompatibilityReady, securityBaselineReady, operationalGoLiveReady, supportedCircuits, readiness);

        var checklist = new List<ZuluAppReplacementChecklistItemDto>
        {
            BuildChecklist("DATOS_BASE", "Smoke de maestros base", smokeReady, $"Sucursales={coreCounts["Sucursales"]}, Usuarios={coreCounts["Usuarios"]}, Terceros={coreCounts["Terceros"]}, Items={coreCounts["Items"]}"),
            BuildChecklist("LEGACY", "Compatibilidad de layouts/tabulares", legacyCompatibilityReady, $"Circuitos={supportedCircuits.Count}, LayoutLegacyProfile={batchSettings.LayoutLegacyProfile}"),
            BuildChecklist("SEGURIDAD", "Baseline de permisos y menú", securityBaselineReady, $"Permisos={coreCounts["Permisos"]}, PermisosUsuario={coreCounts["PermisosUsuario"]}, MenusUsuario={coreCounts["MenusUsuario"]}"),
            BuildChecklist("OPERACION", "Readiness operativo batch/spool/fiscal", operationalGoLiveReady, $"ReadyForGoLive={readiness.ReadyForGoLive}"),
            BuildChecklist("VERTICALES", "Cobertura de negocio migrada", coreCounts["Contratos"] > 0 || coreCounts["Cedulones"] > 0 || coreCounts["OrdenesPreparacion"] > 0, $"Contratos={coreCounts["Contratos"]}, Cedulones={coreCounts["Cedulones"]}, OrdenesPreparacion={coreCounts["OrdenesPreparacion"]}"),
            BuildPendingChecklist("UAT", "UAT con usuarios clave", "Requiere validación funcional sobre entorno real."),
            BuildPendingChecklist("CORTE", "Plan de corte/fallback/rollback", "Requiere definición y validación operativa en producción."),
            BuildPendingChecklist("PARIDAD", "Checklist final contra zuluApp", "Requiere contraste funcional con usuarios y datos reales.")
        };

        var gaps = new List<ZuluAppReplacementGapDto>();
        if (!smokeReady)
            gaps.Add(new ZuluAppReplacementGapDto { Severity = "HIGH", Area = "DATOS_BASE", Description = "Faltan maestros mínimos para smoke de reemplazo." });
        if (!legacyCompatibilityReady)
            gaps.Add(new ZuluAppReplacementGapDto { Severity = "HIGH", Area = "LEGACY", Description = "No está cerrada la compatibilidad local de layouts/tabulares legacy." });
        if (!securityBaselineReady)
            gaps.Add(new ZuluAppReplacementGapDto { Severity = "MEDIUM", Area = "SEGURIDAD", Description = "Falta baseline suficiente de permisos/menú por usuario." });
        if (!operationalGoLiveReady)
            gaps.AddRange(readiness.Issues.Select(x => new ZuluAppReplacementGapDto { Severity = "HIGH", Area = "OPERACION", Description = x }));

        gaps.Add(new ZuluAppReplacementGapDto { Severity = "PENDING_REAL", Area = "MIGRACION", Description = "Falta smoke/migración con base real del cliente." });
        gaps.Add(new ZuluAppReplacementGapDto { Severity = "PENDING_REAL", Area = "UAT", Description = "Falta UAT con usuarios clave y cierre funcional real." });
        gaps.Add(new ZuluAppReplacementGapDto { Severity = "PENDING_REAL", Area = "CORTE", Description = "Falta plan de corte, fallback y rollback validado." });
        gaps.Add(new ZuluAppReplacementGapDto { Severity = "PENDING_REAL", Area = "ACTA", Description = "Falta acta final de go-live y aprobación formal." });

        var recommendations = new List<string>();
        if (!smokeReady)
            recommendations.Add("Completar carga o migración de maestros mínimos y reejecutar smoke local.");
        if (!legacyCompatibilityReady)
            recommendations.Add("Homologar archivos legacy reales por circuito usando plantillas y validación de layout.");
        if (!securityBaselineReady)
            recommendations.Add("Completar asignación de permisos, menú y parámetros operativos por usuario.");
        if (!operationalGoLiveReady)
            recommendations.Add("Resolver issues de readiness operativo antes de declarar reemplazo listo.");
        recommendations.Add("Ejecutar smoke con base real y registrar evidencias circuito por circuito.");
        recommendations.Add("Cerrar checklist de paridad contra zuluApp con usuarios clave.");
        recommendations.Add("Definir y ensayar plan de corte/fallback/rollback previo a producción.");

        return new ZuluAppReplacementCertificationDto
        {
            SmokeReady = smokeReady,
            LegacyCompatibilityReady = legacyCompatibilityReady,
            SecurityBaselineReady = securityBaselineReady,
            OperationalGoLiveReady = operationalGoLiveReady,
            ReplacementReady = replacementReady,
            CoreCounts = coreCounts,
            SupportedLegacyCircuits = supportedCircuits,
            ModuleSmoke = moduleSmoke,
            ModuleParity = moduleParity,
            EvidenceChecklist = evidenceChecklist,
            Checklist = checklist.AsReadOnly(),
            Gaps = gaps.AsReadOnly(),
            RecommendedActions = recommendations.AsReadOnly()
        };
    }

    private static ZuluAppReplacementChecklistItemDto BuildChecklist(string area, string item, bool ok, string detail)
        => new()
        {
            Area = area,
            Item = item,
            Status = ok ? "OK" : "ATTENTION",
            Detail = detail
        };

    private static ZuluAppReplacementChecklistItemDto BuildPendingChecklist(string area, string item, string detail)
        => new()
        {
            Area = area,
            Item = item,
            Status = "PENDING_REAL",
            Detail = detail
        };

    public async Task<ReporteTabularDto> BuildCertificationReportAsync(CancellationToken ct = default)
    {
        var certification = await EvaluateAsync(ct);
        var filas = certification.Checklist.Select(x => (IReadOnlyList<string>)
        [
            x.Area,
            x.Item,
            x.Status,
            x.Detail
        ]).ToList();

        filas.AddRange(certification.Gaps.Select(x => (IReadOnlyList<string>)
        [
            x.Area,
            x.Description,
            x.Severity,
            "GAP"
        ]));

        filas.AddRange(certification.ModuleSmoke.Select(x => (IReadOnlyList<string>)
        [
            $"SMOKE:{x.Module}",
            x.Detail,
            x.Status,
            "MODULE"
        ]));

        filas.AddRange(certification.ModuleParity.Select(x => (IReadOnlyList<string>)
        [
            $"PARIDAD:{x.Module}",
            x.Detail,
            x.Status,
            "MODULE"
        ]));

        filas.AddRange(certification.EvidenceChecklist.Select(x => (IReadOnlyList<string>)
        [
            $"EVIDENCIA:{x.Area}",
            x.Evidence,
            x.Status,
            x.Source
        ]));

        return new ReporteTabularDto
        {
            Titulo = "Certificacion Reemplazo zuluApp",
            Parametros = new Dictionary<string, string>
            {
                ["ReplacementReady"] = certification.ReplacementReady ? "SI" : "NO",
                ["SmokeReady"] = certification.SmokeReady ? "SI" : "NO",
                ["LegacyCompatibilityReady"] = certification.LegacyCompatibilityReady ? "SI" : "NO",
                ["SecurityBaselineReady"] = certification.SecurityBaselineReady ? "SI" : "NO",
                ["OperationalGoLiveReady"] = certification.OperationalGoLiveReady ? "SI" : "NO",
                ["LegacyCircuits"] = string.Join(",", certification.SupportedLegacyCircuits)
            },
            Columnas = ["Area", "Item", "Estado", "Detalle"],
            Filas = filas.AsReadOnly()
        };
    }

    public async Task<IReadOnlyList<ZuluAppModuleSmokeDto>> BuildModuleSmokeAsync(CancellationToken ct = default)
        => (await EvaluateAsync(ct)).ModuleSmoke;

    public async Task<IReadOnlyList<ZuluAppModuleParityDto>> BuildModuleParityAsync(CancellationToken ct = default)
        => (await EvaluateAsync(ct)).ModuleParity;

    public async Task<IReadOnlyList<ZuluAppReplacementEvidenceDto>> BuildEvidenceChecklistAsync(CancellationToken ct = default)
        => (await EvaluateAsync(ct)).EvidenceChecklist;

    public async Task<ZuluAppCutoverPlanDto> BuildCutoverPlanAsync(CancellationToken ct = default)
    {
        var certification = await EvaluateAsync(ct);
        var status = certification.ReplacementReady ? "READY" : "PENDING";

        return new ZuluAppCutoverPlanDto
        {
            Status = status,
            Preconditions =
            [
                new() { Phase = "PRE", Order = 1, Step = "Validar smoke local y reporte de certificacion de reemplazo.", Type = certification.SmokeReady ? "OK" : "PENDING" },
                new() { Phase = "PRE", Order = 2, Step = "Confirmar compatibilidad legacy y layouts por circuito.", Type = certification.LegacyCompatibilityReady ? "OK" : "PENDING" },
                new() { Phase = "PRE", Order = 3, Step = "Confirmar baseline de seguridad, menu y permisos por usuario.", Type = certification.SecurityBaselineReady ? "OK" : "PENDING" },
                new() { Phase = "PRE", Order = 4, Step = "Confirmar readiness operativo batch/spool/fiscal.", Type = certification.OperationalGoLiveReady ? "OK" : "PENDING" }
            ],
            CutoverSteps =
            [
                new() { Phase = "CUTOVER", Order = 1, Step = "Congelar operaciones de zuluApp y registrar hora de corte.", Type = "MANUAL" },
                new() { Phase = "CUTOVER", Order = 2, Step = "Ejecutar smoke con base real en ZuluIA_Back.", Type = "MANUAL" },
                new() { Phase = "CUTOVER", Order = 3, Step = "Verificar jobs, spool, integraciones y dashboard operativo final.", Type = "MANUAL" },
                new() { Phase = "CUTOVER", Order = 4, Step = "Habilitar operacion de usuarios clave y monitorear primeras transacciones.", Type = "MANUAL" }
            ],
            FallbackSteps =
            [
                new() { Phase = "FALLBACK", Order = 1, Step = "Bloquear nuevos ingresos en ZuluIA_Back si se detecta incidente critico.", Type = "MANUAL" },
                new() { Phase = "FALLBACK", Order = 2, Step = "Mantener trazabilidad de correlation id, errores y operaciones afectadas.", Type = "MANUAL" },
                new() { Phase = "FALLBACK", Order = 3, Step = "Redirigir operacion temporal al circuito anterior definido por negocio.", Type = "MANUAL" }
            ],
            RollbackSteps =
            [
                new() { Phase = "ROLLBACK", Order = 1, Step = "Detener jobs automaticos y spool operativo.", Type = "MANUAL" },
                new() { Phase = "ROLLBACK", Order = 2, Step = "Revertir habilitacion operativa y comunicar rollback a usuarios clave.", Type = "MANUAL" },
                new() { Phase = "ROLLBACK", Order = 3, Step = "Registrar evidencia del incidente, gap y decision de rollback.", Type = "MANUAL" }
            ]
        };
    }

    public async Task<ZuluAppGoLiveMinutesTemplateDto> BuildGoLiveMinutesTemplateAsync(CancellationToken ct = default)
    {
        var certification = await EvaluateAsync(ct);
        return new ZuluAppGoLiveMinutesTemplateDto
        {
            Status = certification.ReplacementReady ? "READY_FOR_SIGNATURE" : "PENDING_ITEMS",
            Sections =
            [
                new() { Title = "Objetivo", Content = "Dejar evidencia formal del reemplazo operativo de zuluApp por ZuluIA_Back." },
                new() { Title = "Estado de certificacion", Content = $"ReplacementReady={certification.ReplacementReady}; SmokeReady={certification.SmokeReady}; LegacyCompatibilityReady={certification.LegacyCompatibilityReady}; SecurityBaselineReady={certification.SecurityBaselineReady}; OperationalGoLiveReady={certification.OperationalGoLiveReady}." },
                new() { Title = "Circuitos legacy soportados", Content = certification.SupportedLegacyCircuits.Count == 0 ? "Sin circuitos registrados." : string.Join(", ", certification.SupportedLegacyCircuits) },
                new() { Title = "Gaps pendientes", Content = certification.Gaps.Count == 0 ? "Sin gaps detectados localmente." : string.Join(" | ", certification.Gaps.Select(x => $"[{x.Severity}] {x.Area}: {x.Description}")) },
                new() { Title = "Acciones previas al go-live", Content = string.Join(" | ", certification.RecommendedActions) },
                new() { Title = "Aprobaciones", Content = "Completar con responsables funcionales, técnicos y operativos antes del go-live." }
            ]
        };
    }

    private static IReadOnlyList<ZuluAppModuleSmokeDto> BuildModuleSmoke(IReadOnlyDictionary<string, long> coreCounts, bool smokeReady, bool securityBaselineReady, bool operationalGoLiveReady)
        =>
        [
            BuildModuleSmokeItem("BASE", smokeReady, $"Sucursales={coreCounts["Sucursales"]}, Usuarios={coreCounts["Usuarios"]}, Terceros={coreCounts["Terceros"]}, Items={coreCounts["Items"]}"),
            BuildModuleSmokeItem("VENTAS", coreCounts["Comprobantes"] > 0, $"Comprobantes={coreCounts["Comprobantes"]}"),
            BuildModuleSmokeItem("FINANZAS", coreCounts["Cobros"] > 0 || coreCounts["Pagos"] > 0, $"Cobros={coreCounts["Cobros"]}, Pagos={coreCounts["Pagos"]}"),
            BuildModuleSmokeItem("CONTRATOS", coreCounts["Contratos"] > 0, $"Contratos={coreCounts["Contratos"]}"),
            BuildModuleSmokeItem("COLEGIO", coreCounts["Cedulones"] > 0, $"Cedulones={coreCounts["Cedulones"]}"),
            BuildModuleSmokeItem("LOGISTICA", coreCounts["OrdenesPreparacion"] > 0 || coreCounts["TransferenciasDeposito"] > 0, $"OrdenesPreparacion={coreCounts["OrdenesPreparacion"]}, Transferencias={coreCounts["TransferenciasDeposito"]}"),
            BuildModuleSmokeItem("RRHH", coreCounts["Empleados"] > 0, $"Empleados={coreCounts["Empleados"]}"),
            BuildModuleSmokeItem("SEGURIDAD", securityBaselineReady, $"Permisos={coreCounts["Permisos"]}, PermisosUsuario={coreCounts["PermisosUsuario"]}, MenusUsuario={coreCounts["MenusUsuario"]}"),
            BuildModuleSmokeItem("OPERACION", operationalGoLiveReady, "Readiness batch/spool/fiscal evaluado.")
        ];

    private static IReadOnlyList<ZuluAppModuleParityDto> BuildModuleParity(IReadOnlyDictionary<string, long> coreCounts, bool legacyCompatibilityReady, bool securityBaselineReady, bool operationalGoLiveReady)
        =>
        [
            BuildModuleParityItem("LEGACY", legacyCompatibilityReady ? "ATTENTION" : "PENDING_REAL", "La paridad legacy requiere contraste con archivos y datos reales del cliente."),
            BuildModuleParityItem("SEGURIDAD", securityBaselineReady ? "ATTENTION" : "PENDING", "La seguridad base está modelada, pero requiere validación funcional con perfiles reales."),
            BuildModuleParityItem("OPERACION", operationalGoLiveReady ? "ATTENTION" : "PENDING", "La operación local está preparada, pero la paridad final depende de runbooks y operación real."),
            BuildModuleParityItem("VERTICALES", coreCounts["Contratos"] > 0 || coreCounts["Cedulones"] > 0 || coreCounts["OrdenesPreparacion"] > 0 ? "ATTENTION" : "PENDING", "La cobertura de negocio existe localmente, pero requiere comparación circuito por circuito con zuluApp."),
            BuildModuleParityItem("UAT", "PENDING_REAL", "Requiere validación con usuarios clave."),
            BuildModuleParityItem("CORTE", "PENDING_REAL", "Requiere definición y ensayo del corte/fallback/rollback.")
        ];

    private static ZuluAppModuleSmokeDto BuildModuleSmokeItem(string module, bool ok, string detail)
        => new()
        {
            Module = module,
            Status = ok ? "OK" : "ATTENTION",
            Detail = detail
        };

    private static ZuluAppModuleParityDto BuildModuleParityItem(string module, string status, string detail)
        => new()
        {
            Module = module,
            Status = status,
            Detail = detail
        };

    private static IReadOnlyList<ZuluAppReplacementEvidenceDto> BuildEvidenceChecklist(bool smokeReady, bool legacyCompatibilityReady, bool securityBaselineReady, bool operationalGoLiveReady, IReadOnlyList<string> supportedCircuits, GoLiveOperativoReadinessDto readiness)
        =>
        [
            BuildEvidenceItem("SMOKE", "Reporte de smoke de maestros y modulos base", smokeReady ? "OK" : "ATTENTION", "DB/CONFIG"),
            BuildEvidenceItem("LEGACY", $"Circuitos legacy soportados: {string.Join(", ", supportedCircuits)}", legacyCompatibilityReady ? "OK" : "ATTENTION", "LAYOUT_PROFILE"),
            BuildEvidenceItem("SEGURIDAD", "Baseline de permisos, menu y parametros por usuario", securityBaselineReady ? "OK" : "ATTENTION", "SEGURIDAD"),
            BuildEvidenceItem("OPERACION", $"Readiness operativo: {string.Join(" | ", readiness.Issues.DefaultIfEmpty("Sin issues locales."))}", operationalGoLiveReady ? "OK" : "ATTENTION", "GO_LIVE"),
            BuildEvidenceItem("UAT", "Evidencia funcional con usuarios clave", "PENDING_REAL", "MANUAL"),
            BuildEvidenceItem("CORTE", "Evidencia de corte/fallback/rollback ensayado", "PENDING_REAL", "MANUAL"),
            BuildEvidenceItem("ACTA", "Acta final firmada de go-live", "PENDING_REAL", "MANUAL")
        ];

    private static ZuluAppReplacementEvidenceDto BuildEvidenceItem(string area, string evidence, string status, string source)
        => new()
        {
            Area = area,
            Evidence = evidence,
            Status = status,
            Source = source
        };
}
