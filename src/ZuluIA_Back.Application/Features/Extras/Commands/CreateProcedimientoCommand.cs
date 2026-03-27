using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreateProcedimientoCommand(
    string Nombre,
    string DefinicionJson,
    long? UsuarioId,
    bool EsGlobal) : IRequest<Result<long>>;

public record UpdateProcedimientoCommand(
    long Id,
    string Nombre,
    string DefinicionJson,
    bool EsGlobal) : IRequest<Result>;

public record DeleteProcedimientoCommand(long Id) : IRequest<Result>;
