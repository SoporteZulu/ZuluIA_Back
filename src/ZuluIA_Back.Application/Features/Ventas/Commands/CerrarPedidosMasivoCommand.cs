using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public record CerrarPedidosMasivoCommand(
    long? SucursalId,
    long? TerceroId,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta,
    DateOnly? FechaEntregaDesde,
    DateOnly? FechaEntregaHasta,
    bool SoloPendientes,
    string? MotivoCierre) : IRequest<Result<int>>;
