using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public record CrearRequisicionItemDto(
    long? ItemId,
    string Descripcion,
    decimal Cantidad,
    string UnidadMedida,
    string? Observacion);

public record CrearRequisicionCompraCommand(
    long SucursalId,
    long SolicitanteId,
    DateOnly Fecha,
    string Descripcion,
    string? Observacion,
    IReadOnlyList<CrearRequisicionItemDto> Items)
    : IRequest<Result<long>>;
