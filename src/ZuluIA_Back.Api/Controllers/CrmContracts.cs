namespace ZuluIA_Back.Api.Controllers;

internal sealed record CrmCatalogOptionResponse(string Id, string Nombre);
internal sealed record CrmCatalogoClienteOptionResponse(string Id, string Nombre, string TipoCliente, string Segmento, string EstadoRelacion);
internal sealed record CrmCatalogoContactoOptionResponse(string Id, string ClienteId, string Nombre, string? Cargo, string EstadoContacto);
internal sealed record CrmCatalogoUsuarioOptionResponse(string Id, string Nombre, string Rol);
internal sealed record CrmCatalogoSegmentoOptionResponse(string Id, string Nombre, string TipoSegmento);
internal sealed record CrmTipoRelacionResponse(string Id, string Codigo, string Descripcion, bool Activo);
internal sealed record CrmCatalogosResponse(
    IReadOnlyList<CrmCatalogOptionResponse> TiposCliente,
    IReadOnlyList<CrmCatalogOptionResponse> SegmentosCliente,
    IReadOnlyList<CrmCatalogOptionResponse> OrigenesCliente,
    IReadOnlyList<CrmCatalogOptionResponse> EstadosRelacion,
    IReadOnlyList<CrmCatalogOptionResponse> CanalesContacto,
    IReadOnlyList<CrmCatalogOptionResponse> EstadosContacto,
    IReadOnlyList<CrmCatalogOptionResponse> EtapasOportunidad,
    IReadOnlyList<CrmCatalogOptionResponse> Monedas,
    IReadOnlyList<CrmCatalogOptionResponse> OrigenesOportunidad,
    IReadOnlyList<CrmCatalogOptionResponse> TiposInteraccion,
    IReadOnlyList<CrmCatalogOptionResponse> CanalesInteraccion,
    IReadOnlyList<CrmCatalogOptionResponse> ResultadosInteraccion,
    IReadOnlyList<CrmCatalogOptionResponse> TiposTarea,
    IReadOnlyList<CrmCatalogOptionResponse> PrioridadesTarea,
    IReadOnlyList<CrmCatalogOptionResponse> EstadosTarea,
    IReadOnlyList<CrmCatalogOptionResponse> TiposCampana,
    IReadOnlyList<CrmCatalogOptionResponse> ObjetivosCampana,
    IReadOnlyList<CrmCatalogOptionResponse> TiposSegmento,
    IReadOnlyList<CrmCatalogOptionResponse> RolesUsuario,
    IReadOnlyList<CrmCatalogOptionResponse> EstadosUsuario,
    IReadOnlyList<CrmTipoRelacionResponse> TiposRelacionContacto,
    IReadOnlyList<CrmCatalogoClienteOptionResponse> Clientes,
    IReadOnlyList<CrmCatalogoContactoOptionResponse> Contactos,
    IReadOnlyList<CrmCatalogoUsuarioOptionResponse> Usuarios,
    IReadOnlyList<CrmCatalogoSegmentoOptionResponse> Segmentos);

internal sealed record CrmClienteResponse(
    string Id,
    string Nombre,
    string TipoCliente,
    string Segmento,
    string? Industria,
    string? Cuit,
    string Pais,
    string? Provincia,
    string? Ciudad,
    string? Direccion,
    string? TelefonoPrincipal,
    string? EmailPrincipal,
    string? SitioWeb,
    string OrigenCliente,
    string EstadoRelacion,
    string? ResponsableId,
    DateOnly FechaAlta,
    string? NotasGenerales,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

internal sealed record CrmContactoResponse(
    string Id,
    string ClienteId,
    string Nombre,
    string Apellido,
    string? Cargo,
    string? Email,
    string? Telefono,
    string CanalPreferido,
    string EstadoContacto,
    string? Notas,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

internal sealed record CrmOportunidadResponse(
    string Id,
    string ClienteId,
    string? ContactoPrincipalId,
    string Titulo,
    string Etapa,
    int Probabilidad,
    decimal MontoEstimado,
    string Moneda,
    DateOnly FechaApertura,
    DateOnly? FechaEstimadaCierre,
    string? ResponsableId,
    string Origen,
    string? MotivoPerdida,
    string? Notas,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

internal sealed record CrmInteraccionResponse(
    string Id,
    string ClienteId,
    string? ContactoId,
    string? OportunidadId,
    string TipoInteraccion,
    string Canal,
    DateTimeOffset FechaHora,
    string UsuarioResponsableId,
    string Resultado,
    string? Descripcion,
    IReadOnlyList<string> Adjuntos,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

internal sealed record CrmTareaResponse(
    string Id,
    string? ClienteId,
    string? OportunidadId,
    string AsignadoAId,
    string Titulo,
    string? Descripcion,
    string TipoTarea,
    DateOnly FechaVencimiento,
    string Prioridad,
    string Estado,
    DateOnly? FechaCompletado,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

internal sealed record CrmSegmentoMiembroResponse(
    string Id,
    string Nombre,
    string TipoCliente,
    string Segmento,
    string? Industria,
    string OrigenCliente,
    string EstadoRelacion,
    string Pais,
    string? Provincia,
    string? Ciudad);

internal sealed record CrmSegmentoResponse(
    string Id,
    string Nombre,
    string? Descripcion,
    IReadOnlyList<CrmSegmentCriterionRequest> Criterios,
    string TipoSegmento,
    int CantidadClientes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

internal sealed record CrmUsuarioResponse(
    string Id,
    string Nombre,
    string Apellido,
    string? Email,
    string Rol,
    string Estado,
    string? Avatar,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

internal sealed record CrmCampanaResponse(
    string Id,
    long SucursalId,
    string Nombre,
    string? TipoCampana,
    string? Objetivo,
    string? SegmentoObjetivoId,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    decimal PresupuestoEstimado,
    decimal PresupuestoGastado,
    string? ResponsableId,
    string? Notas,
    int LeadsGenerados,
    int OportunidadesGeneradas,
    int NegociosGanados,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool Activa);

internal sealed record CrmResumenComercialResponse(int ClientesActivos, decimal PipelineAbierto, int CierresVencidos, int SeguimientoVencido);
internal sealed record CrmPipelineEtapaResponse(string Etapa, int Cantidad, decimal Monto);
internal sealed record CrmProbabilidadResponse(string Rango, int Cantidad);
internal sealed record CrmRankingVendedorResponse(string UsuarioId, string Nombre, int OportunidadesGanadas, decimal MontoGanado, int OportunidadesActivas, decimal Pipeline);
internal sealed record CrmRadarOportunidadResponse(string Id, string Titulo, string Cliente, string Responsable, decimal MontoEstimado, DateOnly? FechaEstimadaCierre, DateTimeOffset? UltimaGestion, string Origen, int Riesgo);
internal sealed record CrmRadarClienteResponse(string Id, string Nombre, string Responsable, string Segmento, DateTimeOffset? UltimaGestion, decimal Pipeline, string EstadoRelacion, int Criticidad);
internal sealed record CrmResumenMarketingResponse(int CampanasActivas, decimal DesvioPresupuestario, int Leads, decimal Conversion);
internal sealed record CrmDistribucionResponse(string Nombre, int Cantidad);
internal sealed record CrmResultadoCampanaResponse(string Id, string Nombre, string TipoCampana, decimal Presupuesto, decimal Gastado, int LeadsGenerados, int OportunidadesGeneradas, int NegociosGanados);
internal sealed record CrmRadarCampanaResponse(string Id, string Nombre, string Objetivo, string Responsable, DateOnly FechaInicio, DateOnly FechaFin, decimal Desvio, decimal CostoPorLead, int OportunidadesGeneradas, decimal TasaConversion);
internal sealed record CrmActividadUsuarioResponse(string UsuarioId, string Nombre, int Llamadas, int Emails, int Reuniones, int Visitas, int Total);
internal sealed record CrmActividadRecienteResponse(string Id, DateTimeOffset FechaHora, string TipoInteraccion, string Canal, string Resultado, string Cliente, string Usuario, string? Descripcion);
internal sealed record CrmReportesResponse(
    CrmResumenComercialResponse ResumenComercial,
    IReadOnlyList<CrmPipelineEtapaResponse> PipelinePorEtapa,
    IReadOnlyList<CrmProbabilidadResponse> DistribucionProbabilidad,
    IReadOnlyList<CrmRankingVendedorResponse> RankingVendedores,
    IReadOnlyList<CrmRadarOportunidadResponse> RadarOportunidades,
    IReadOnlyList<CrmRadarClienteResponse> RadarClientes,
    CrmResumenMarketingResponse ResumenMarketing,
    IReadOnlyList<CrmDistribucionResponse> ClientesPorSegmento,
    IReadOnlyList<CrmDistribucionResponse> ClientesPorIndustria,
    IReadOnlyList<CrmResultadoCampanaResponse> ResultadosCampanas,
    IReadOnlyList<CrmRadarCampanaResponse> RadarCampanas,
    IReadOnlyList<CrmActividadUsuarioResponse> ActividadPorUsuario,
    IReadOnlyList<CrmActividadRecienteResponse> ActividadReciente);
