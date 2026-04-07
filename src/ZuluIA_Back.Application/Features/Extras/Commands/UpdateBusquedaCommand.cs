using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record UpdateBusquedaCommand(
    long Id,
    string Nombre,
    string FiltrosJson,
    bool EsGlobal) : IRequest<Result>;

public record DeleteBusquedaCommand(long Id) : IRequest<Result>;
