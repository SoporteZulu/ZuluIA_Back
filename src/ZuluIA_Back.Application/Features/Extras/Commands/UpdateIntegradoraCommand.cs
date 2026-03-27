using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record UpdateIntegradoraCommand(
    long Id,
    string Nombre,
    string TipoSistema,
    string? UrlEndpoint,
    string? Configuracion) : IRequest<Result>;