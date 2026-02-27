using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record CreateAsientoCommand(
    long EjercicioId,
    long SucursalId,
    DateOnly Fecha,
    string Descripcion,
    string? OrigenTabla,
    long? OrigenId,
    List<CreateAsientoLineaDto> Lineas
) : IRequest<Result<long>>;