using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

/// <summary>
/// Handler para la baja lógica de un tercero.
/// Implementa el validarEliminar() del VB6 antes de ejecutar Desactivar().
/// </summary>
public class DeleteTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteTerceroCommand, Result>
{
    public async Task<Result> Handle(
        DeleteTerceroCommand command,
        CancellationToken ct)
    {
        // ── 1. Obtener entidad ─────────────────────────────────────────────────
        var tercero = await repo.GetByIdAsync(command.Id, ct);

        if (tercero is null)
            return Result.Failure(
                $"No se encontró el tercero con Id {command.Id}.");

        // ── 2. Verificar que no esté ya dado de baja ───────────────────────────
        if (tercero.IsDeleted)
            return Result.Failure(
                $"El tercero '{tercero.Legajo} — {tercero.RazonSocial}' " +
                "ya está dado de baja.");

        // ── 3. validarEliminar() — Integridad referencial ──────────────────────
        // Bloque equivalente exacto al validarEliminar() del VB6 que hacía
        // SELECT COUNT(*) en cada tabla relacionada antes de permitir la baja.

        // 3a. Comprobantes (ventas y compras)
        if (await repo.TieneComprobantesAsync(command.Id, ct))
            return Result.Failure(
                $"No se puede dar de baja a '{tercero.RazonSocial}' porque " +
                "tiene comprobantes asociados (facturas, remitos, etc.).");

        // 3b. Movimientos de cuenta corriente
        if (await repo.TieneMovimientosCuentaCorrienteAsync(command.Id, ct))
            return Result.Failure(
                $"No se puede dar de baja a '{tercero.RazonSocial}' porque " +
                "tiene movimientos en cuenta corriente.");

        // 3c. Empleado activo
        if (tercero.EsEmpleado &&
            await repo.TieneEmpleadoActivoAsync(command.Id, ct))
            return Result.Failure(
                $"No se puede dar de baja a '{tercero.RazonSocial}' porque " +
                "tiene un legajo laboral activo. Dé de baja el empleado primero.");

        // ── 4. Ejecutar baja lógica ────────────────────────────────────────────
        // Desactivar() setea activo=false, deleted_at=now() y dispara
        // TerceroDesactivadoEvent.
        tercero.Desactivar(currentUser.UserId);

        // ── 5. Persistir ───────────────────────────────────────────────────────
        repo.Update(tercero);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

/// <summary>
/// Handler para la reactivación de un tercero dado de baja.
/// </summary>
public class ActivarTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivarTerceroCommand, Result>
{
    public async Task<Result> Handle(
        ActivarTerceroCommand command,
        CancellationToken ct)
    {
        // ── 1. Obtener entidad ─────────────────────────────────────────────────
        // GetByIdAsync del BaseRepository usa FindAsync que no filtra por
        // deleted_at — por eso podemos recuperar terceros dados de baja.
        var tercero = await repo.GetByIdAsync(command.Id, ct);

        if (tercero is null)
            return Result.Failure(
                $"No se encontró el tercero con Id {command.Id}.");

        // ── 2. Verificar que esté efectivamente dado de baja ───────────────────
        if (!tercero.IsDeleted && tercero.Activo)
            return Result.Failure(
                $"El tercero '{tercero.Legajo} — {tercero.RazonSocial}' " +
                "ya se encuentra activo.");

        // ── 3. Reactivar ───────────────────────────────────────────────────────
        // Activar() setea activo=true, limpia deleted_at y dispara
        // TerceroReactivadoEvent.
        tercero.Activar(currentUser.UserId);

        // ── 4. Persistir ───────────────────────────────────────────────────────
        repo.Update(tercero);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}