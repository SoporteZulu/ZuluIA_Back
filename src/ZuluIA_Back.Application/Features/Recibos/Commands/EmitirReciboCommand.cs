using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Recibos.Commands;

public record EmitirReciboItemDto(string Descripcion, decimal Importe);

public record EmitirReciboCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    string Serie,
    string? Observacion,
    long? CobroId,
    IReadOnlyList<EmitirReciboItemDto> Items)
    : IRequest<Result<long>>;
