using MediatR;
using ZuluIA_Back.Application.Features.Configuracion.DTOs;

namespace ZuluIA_Back.Application.Features.Configuracion.Queries;

/// <summary>
/// Retorna todos los parámetros de configuración del sistema.
/// </summary>
public record GetConfiguracionQuery : IRequest<IReadOnlyList<ConfiguracionDto>>;