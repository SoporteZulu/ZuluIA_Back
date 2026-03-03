using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Configuracion.Commands;

/// <summary>
/// Crea o actualiza (upsert) un parámetro de configuración del sistema.
/// </summary>
public record SetConfiguracionCommand(string Campo, string? Valor) : IRequest<Result>;