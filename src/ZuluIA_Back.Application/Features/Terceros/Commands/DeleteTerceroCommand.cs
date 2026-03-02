using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

/// <summary>
/// Da de baja lógica (soft delete) a un tercero.
/// Equivalente al eliminar() + validarEliminar() del VB6.
///
/// La baja es SIEMPRE lógica (soft delete):
///   - activo = false
///   - deleted_at = now()
/// El registro se preserva por integridad referencial con
/// comprobantes, movimientos de cuenta corriente, etc.
///
/// Se incluye también ActivarTerceroCommand en este archivo
/// porque son operaciones de ciclo de vida complementarias
/// (baja / reactivación) y comparten el mismo contexto.
/// </summary>
public record DeleteTerceroCommand(long Id) : IRequest<Result>;

/// <summary>
/// Reactiva un tercero previamente dado de baja.
/// No existía como acción explícita en el VB6 (se hacía directo en BD),
/// pero es necesaria para el flujo de administración de la API.
/// </summary>
public record ActivarTerceroCommand(long Id) : IRequest<Result>;