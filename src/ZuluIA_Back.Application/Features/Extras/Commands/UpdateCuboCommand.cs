using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record UpdateCuboCommand(
    long Id,
    string Descripcion,
    string? OrigenDatos,
    string? Observacion,
    int? AmbienteId,
    long? MenuCuboId,
    long? FormatoId) : IRequest<Result>;
