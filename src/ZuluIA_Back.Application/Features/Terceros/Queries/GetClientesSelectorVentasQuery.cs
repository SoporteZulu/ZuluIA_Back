using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Consulta optimizada de clientes para selector de ventas.
/// Retorna lista acotada (no paginada) con filtros específicos para ventas.
/// Equivalente al combo/autocomplete de cliente en Pedidos/Remitos/Facturas del VB6.
/// </summary>
public record GetClientesSelectorVentasQuery(
    string? Search = null,
    long? SucursalId = null,
    int MaxResults = 50
) : IRequest<IReadOnlyList<ClienteSelectorVentasDto>>;
