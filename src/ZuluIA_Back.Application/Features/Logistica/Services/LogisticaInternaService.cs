using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Logistica.Services;

public class LogisticaInternaService(
    IApplicationDbContext db,
    IRepository<OrdenPreparacion> ordenRepo,
    IRepository<LogisticaInternaEvento> eventoRepo,
    IRepository<TransferenciaDeposito> transferenciaRepo,
    StockService stockService,
    ICurrentUserService currentUser)
{
    public async Task<OrdenPreparacion> CrearOrdenPreparacionAsync(CreateOrdenPreparacionCommand request, CancellationToken ct)
    {
        if (!request.Detalles.Any())
            throw new InvalidOperationException("La orden de preparación debe tener al menos un detalle.");

        await ValidarOrdenPreparacionAsync(request.SucursalId, request.ComprobanteOrigenId, request.TerceroId, request.Detalles, ct);

        var orden = OrdenPreparacion.Crear(request.SucursalId, request.ComprobanteOrigenId, request.TerceroId, request.Fecha, request.Observacion, currentUser.UserId);
        db.OrdenesPreparacion.Add(orden);
        await db.SaveChangesAsync(ct);

        foreach (var d in request.Detalles)
            orden.AgregarDetalle(d.ItemId, d.DepositoId, d.Cantidad, d.Observacion);

        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(orden.Id, null, TipoEventoLogisticaInterna.CreacionOrdenPreparacion, request.Fecha, $"Orden de preparación #{orden.Id} creada.", currentUser.UserId), ct);
        ordenRepo.Update(orden);
        return orden;
    }

    public async Task IniciarOrdenPreparacionAsync(long id, CancellationToken ct)
    {
        var orden = await db.OrdenesPreparacion.Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la orden de preparación con ID {id}.");

        orden.IniciarPreparacion(currentUser.UserId);
        ordenRepo.Update(orden);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(orden.Id, null, TipoEventoLogisticaInterna.InicioPicking, DateOnly.FromDateTime(DateTime.Today), $"Inicio de picking de la orden #{orden.Id}.", currentUser.UserId), ct);
    }

    public async Task RegistrarPickingAsync(long id, IReadOnlyList<RegistrarPickingDetalleInput> detalles, CancellationToken ct)
    {
        var orden = await db.OrdenesPreparacion.Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la orden de preparación con ID {id}.");

        foreach (var detalle in detalles)
            orden.RegistrarPicking(detalle.DetalleId, detalle.CantidadEntregada, currentUser.UserId);

        ordenRepo.Update(orden);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(orden.Id, null, TipoEventoLogisticaInterna.RegistroPicking, DateOnly.FromDateTime(DateTime.Today), $"Picking registrado para la orden #{orden.Id}.", currentUser.UserId), ct);
    }

    public async Task ConfirmarOrdenPreparacionAsync(long id, CancellationToken ct)
    {
        var orden = await db.OrdenesPreparacion.Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la orden de preparación con ID {id}.");

        orden.Confirmar(DateOnly.FromDateTime(DateTime.Today), currentUser.UserId);
        ordenRepo.Update(orden);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(orden.Id, null, TipoEventoLogisticaInterna.ConfirmacionOrdenPreparacion, DateOnly.FromDateTime(DateTime.Today), $"Orden #{orden.Id} confirmada para despacho interno.", currentUser.UserId), ct);
    }

    public async Task AnularOrdenPreparacionAsync(long id, CancellationToken ct)
    {
        var orden = await db.OrdenesPreparacion.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la orden de preparación con ID {id}.");

        orden.Anular(currentUser.UserId);
        ordenRepo.Update(orden);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(orden.Id, null, TipoEventoLogisticaInterna.AnulacionOrdenPreparacion, DateOnly.FromDateTime(DateTime.Today), $"Orden #{orden.Id} anulada.", currentUser.UserId), ct);
    }

    public async Task<TransferenciaDeposito> CrearTransferenciaAsync(Features.TransferenciasDeposito.Commands.CreateTransferenciaDepositoCommand request, CancellationToken ct)
    {
        if (!request.Detalles.Any())
            throw new InvalidOperationException("La transferencia de depósito debe tener al menos un detalle.");

        await ValidarTransferenciaAsync(request.SucursalId, request.DepositoOrigenId, request.DepositoDestinoId, request.Detalles, request.OrdenPreparacionId, ct);

        var transferencia = TransferenciaDeposito.Crear(request.SucursalId, request.DepositoOrigenId, request.DepositoDestinoId, request.Fecha, request.Observacion, currentUser.UserId);
        if (request.OrdenPreparacionId.HasValue)
            transferencia.VincularOrdenPreparacion(request.OrdenPreparacionId.Value, currentUser.UserId);

        db.TransferenciasDeposito.Add(transferencia);
        await db.SaveChangesAsync(ct);

        foreach (var d in request.Detalles)
            transferencia.AgregarDetalle(d.ItemId, d.Cantidad, d.Observacion);

        transferenciaRepo.Update(transferencia);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(null, transferencia.Id, TipoEventoLogisticaInterna.CreacionTransferenciaDeposito, request.Fecha, $"Transferencia #{transferencia.Id} creada.", currentUser.UserId), ct);
        return transferencia;
    }

    public async Task<TransferenciaDeposito> DespacharOrdenPreparacionAsync(long ordenId, long depositoDestinoId, DateOnly fecha, string? observacion, CancellationToken ct)
    {
        var orden = await db.OrdenesPreparacion.Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == ordenId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la orden de preparación con ID {ordenId}.");

        if (orden.Estado != EstadoOrdenPreparacion.Completada)
            throw new InvalidOperationException("Solo se puede despachar internamente una orden de preparación completada.");
        if (!orden.Detalles.Any())
            throw new InvalidOperationException("La orden de preparación no posee detalles para despachar.");
        if (await db.TransferenciasDeposito.AsNoTracking().AnyAsync(x => x.OrdenPreparacionId == ordenId && !x.IsDeleted && x.Estado != EstadoTransferenciaDeposito.Anulada, ct))
            throw new InvalidOperationException("La orden de preparación ya posee una transferencia interna asociada.");

        var depositoOrigenId = orden.Detalles.Select(x => x.DepositoId).Distinct().ToList();
        if (depositoOrigenId.Count != 1)
            throw new InvalidOperationException("Para despacho interno automático, todos los renglones deben pertenecer al mismo depósito origen.");

        var transferencia = TransferenciaDeposito.Crear(orden.SucursalId, depositoOrigenId[0], depositoDestinoId, fecha, observacion ?? $"Despacho interno desde orden #{orden.Id}", currentUser.UserId);
        transferencia.VincularOrdenPreparacion(orden.Id, currentUser.UserId);
        db.TransferenciasDeposito.Add(transferencia);
        await db.SaveChangesAsync(ct);

        foreach (var detalle in orden.Detalles)
            transferencia.AgregarDetalle(detalle.ItemId, detalle.CantidadEntregada, detalle.Observacion);

        transferenciaRepo.Update(transferencia);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(orden.Id, transferencia.Id, TipoEventoLogisticaInterna.DespachoInternoOrdenPreparacion, fecha, $"Despacho interno generado desde la orden #{orden.Id} hacia la transferencia #{transferencia.Id}.", currentUser.UserId), ct);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(orden.Id, transferencia.Id, TipoEventoLogisticaInterna.CreacionTransferenciaDeposito, fecha, $"Transferencia #{transferencia.Id} creada desde la orden #{orden.Id}.", currentUser.UserId), ct);
        return transferencia;
    }

    public async Task ConfirmarTransferenciaAsync(long id, CancellationToken ct)
    {
        var transferencia = await db.TransferenciasDeposito.Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la transferencia de depósito con ID {id}.");

        foreach (var detalle in transferencia.Detalles)
        {
            await stockService.TransferirAsync(detalle.ItemId, transferencia.DepositoOrigenId, transferencia.DepositoDestinoId, detalle.Cantidad, transferencia.Observacion, currentUser.UserId, ct);
        }

        transferencia.Confirmar(DateOnly.FromDateTime(DateTime.Today), currentUser.UserId);
        transferenciaRepo.Update(transferencia);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(transferencia.OrdenPreparacionId, transferencia.Id, TipoEventoLogisticaInterna.ConfirmacionTransferenciaDeposito, DateOnly.FromDateTime(DateTime.Today), transferencia.OrdenPreparacionId.HasValue
            ? $"Transferencia #{transferencia.Id} confirmada para la orden #{transferencia.OrdenPreparacionId.Value}."
            : $"Transferencia #{transferencia.Id} confirmada.", currentUser.UserId), ct);
    }

    public async Task AnularTransferenciaAsync(long id, CancellationToken ct)
    {
        var transferencia = await db.TransferenciasDeposito.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la transferencia de depósito con ID {id}.");

        transferencia.Anular(currentUser.UserId);
        transferenciaRepo.Update(transferencia);
        await eventoRepo.AddAsync(LogisticaInternaEvento.Registrar(transferencia.OrdenPreparacionId, transferencia.Id, TipoEventoLogisticaInterna.AnulacionTransferenciaDeposito, DateOnly.FromDateTime(DateTime.Today), transferencia.OrdenPreparacionId.HasValue
            ? $"Transferencia #{transferencia.Id} anulada para la orden #{transferencia.OrdenPreparacionId.Value}."
            : $"Transferencia #{transferencia.Id} anulada.", currentUser.UserId), ct);
    }

    private async Task ValidarOrdenPreparacionAsync(long sucursalId, long? comprobanteOrigenId, long? terceroId, IReadOnlyList<ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs.CreateOrdenPreparacionDetalleDto> detalles, CancellationToken ct)
    {
        if (!await db.Sucursales.AsNoTracking().AnyAsync(x => x.Id == sucursalId, ct))
            throw new InvalidOperationException($"No se encontró la sucursal ID {sucursalId}.");
        if (comprobanteOrigenId.HasValue && !await db.Comprobantes.AsNoTracking().AnyAsync(x => x.Id == comprobanteOrigenId.Value && !x.IsDeleted, ct))
            throw new InvalidOperationException($"No se encontró el comprobante origen ID {comprobanteOrigenId.Value}.");
        if (terceroId.HasValue && !await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == terceroId.Value && !x.IsDeleted, ct))
            throw new InvalidOperationException($"No se encontró el tercero ID {terceroId.Value}.");

        var itemIds = detalles.Select(x => x.ItemId).Distinct().ToList();
        var depositoIds = detalles.Select(x => x.DepositoId).Distinct().ToList();
        if (await db.Items.AsNoTracking().CountAsync(x => itemIds.Contains(x.Id) && x.Activo, ct) != itemIds.Count)
            throw new InvalidOperationException("Uno o más ítems de la orden de preparación no existen o están inactivos.");
        if (await db.Depositos.AsNoTracking().CountAsync(x => depositoIds.Contains(x.Id) && x.SucursalId == sucursalId, ct) != depositoIds.Count)
            throw new InvalidOperationException("Uno o más depósitos de la orden de preparación no existen o no pertenecen a la sucursal indicada.");
    }

    private async Task ValidarTransferenciaAsync(long sucursalId, long depositoOrigenId, long depositoDestinoId, IReadOnlyList<Features.TransferenciasDeposito.Commands.CreateTransferenciaDepositoDetalleInput> detalles, long? ordenPreparacionId, CancellationToken ct)
    {
        if (!await db.Sucursales.AsNoTracking().AnyAsync(x => x.Id == sucursalId, ct))
            throw new InvalidOperationException($"No se encontró la sucursal ID {sucursalId}.");
        if (!await db.Depositos.AsNoTracking().AnyAsync(x => x.Id == depositoOrigenId && x.SucursalId == sucursalId, ct))
            throw new InvalidOperationException($"No se encontró el depósito origen ID {depositoOrigenId}.");
        if (!await db.Depositos.AsNoTracking().AnyAsync(x => x.Id == depositoDestinoId && x.SucursalId == sucursalId, ct))
            throw new InvalidOperationException($"No se encontró el depósito destino ID {depositoDestinoId}.");

        var itemIds = detalles.Select(x => x.ItemId).Distinct().ToList();
        if (await db.Items.AsNoTracking().CountAsync(x => itemIds.Contains(x.Id) && x.Activo, ct) != itemIds.Count)
            throw new InvalidOperationException("Uno o más ítems de la transferencia no existen o están inactivos.");

        if (ordenPreparacionId.HasValue && !await db.OrdenesPreparacion.AsNoTracking().AnyAsync(x => x.Id == ordenPreparacionId.Value && !x.IsDeleted, ct))
            throw new InvalidOperationException($"No se encontró la orden de preparación ID {ordenPreparacionId.Value}.");
    }
}
