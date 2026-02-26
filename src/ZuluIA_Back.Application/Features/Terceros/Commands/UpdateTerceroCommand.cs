using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record UpdateTerceroCommand(
    long Id,
    string RazonSocial,
    string? NombreFantasia,
    string? Calle,
    string? Nro,
    string? CodigoPostal,
    long? LocalidadId,
    long? BarrioId,
    string? Telefono,
    string? Celular,
    string? Email,
    string? Web,
    decimal? LimiteCredito,
    long? CategoriaId,
    long? MonedaId
) : IRequest<Result>;