using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreateIntegradoraCommand(
    string Codigo,
    string Nombre,
    string TipoSistema,
    string? UrlEndpoint,
    string? ApiKey,
    string? Configuracion) : IRequest<Result<long>>;