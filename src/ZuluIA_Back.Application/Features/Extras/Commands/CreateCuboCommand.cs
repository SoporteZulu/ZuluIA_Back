using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreateCuboCommand(
    string Descripcion,
    string? OrigenDatos,
    string? Observacion,
    int? AmbienteId,
    long? MenuCuboId,
    long? CuboOrigenId,
    bool? EsSistema,
    long? FormatoId,
    long? UsuarioAltaId) : IRequest<Result<long>>;
