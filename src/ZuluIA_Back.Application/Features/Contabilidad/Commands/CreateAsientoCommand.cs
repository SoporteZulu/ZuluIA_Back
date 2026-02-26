using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record CreateAsientoLineaDto(
    long CuentaId,
    decimal Debe,
    decimal Haber,
    string? Descripcion,
    short Orden,
    long? CentroCostoId = null
);

public record CreateAsientoCommand(
    long EjercicioId,
    long SucursalId,
    DateOnly Fecha,
    string Descripcion,
    string? OrigenTabla,
    long? OrigenId,
    List<CreateAsientoLineaDto> Lineas
) : IRequest<Result<long>>;