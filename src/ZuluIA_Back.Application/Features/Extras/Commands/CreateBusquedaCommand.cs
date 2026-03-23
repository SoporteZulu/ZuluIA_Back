using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreateBusquedaCommand(
    string Nombre,
    string Modulo,
    string FiltrosJson,
    long? UsuarioId,
    bool EsGlobal) : IRequest<Result<long>>;
