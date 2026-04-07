using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Tesoreria.Services;

public class TesoreriaService(
    IApplicationDbContext db,
    IRepository<TesoreriaMovimiento> movimientoRepo,
    IRepository<TesoreriaCierre> cierreRepo,
    ICurrentUserService currentUser)
{
    public async Task<TesoreriaMovimiento> RegistrarMovimientoAsync(
        long sucursalId,
        long cajaCuentaId,
        DateOnly fecha,
        TipoOperacionTesoreria tipoOperacion,
        SentidoMovimientoTesoreria sentido,
        decimal importe,
        long monedaId,
        decimal cotizacion,
        long? terceroId,
        string? referenciaTipo,
        long? referenciaId,
        string? observacion,
        CancellationToken ct = default)
    {
        await ValidarCajaOperableAsync(cajaCuentaId, ct);

        var movimiento = TesoreriaMovimiento.Registrar(
            sucursalId,
            cajaCuentaId,
            fecha,
            tipoOperacion,
            sentido,
            importe,
            monedaId,
            cotizacion,
            terceroId,
            referenciaTipo,
            referenciaId,
            observacion,
            currentUser.UserId);

        await movimientoRepo.AddAsync(movimiento, ct);
        return movimiento;
    }

    public async Task<(TesoreriaMovimiento origen, TesoreriaMovimiento destino)> RegistrarMovimientoEntreCajasAsync(
        long sucursalId,
        long cajaOrigenId,
        long cajaDestinoId,
        DateOnly fecha,
        TipoOperacionTesoreria operacionOrigen,
        TipoOperacionTesoreria operacionDestino,
        decimal importe,
        long monedaId,
        decimal cotizacion,
        string? referenciaTipo,
        long? referenciaId,
        string? observacion,
        CancellationToken ct = default)
    {
        var origen = await RegistrarMovimientoAsync(
            sucursalId,
            cajaOrigenId,
            fecha,
            operacionOrigen,
            SentidoMovimientoTesoreria.Egreso,
            importe,
            monedaId,
            cotizacion,
            null,
            referenciaTipo,
            referenciaId,
            observacion,
            ct);

        var destino = await RegistrarMovimientoAsync(
            sucursalId,
            cajaDestinoId,
            fecha,
            operacionDestino,
            SentidoMovimientoTesoreria.Ingreso,
            importe,
            monedaId,
            cotizacion,
            null,
            referenciaTipo,
            referenciaId,
            observacion,
            ct);

        return (origen, destino);
    }

    public async Task<TesoreriaCierre> AbrirCajaAsync(
        long cajaId,
        DateOnly fechaApertura,
        decimal saldoInicial,
        string? observacion,
        CancellationToken ct = default)
    {
        var caja = await db.CajasCuentasBancarias.FirstOrDefaultAsync(x => x.Id == cajaId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la caja con ID {cajaId}.");

        caja.AbrirCaja(fechaApertura, saldoInicial, currentUser.UserId);

        var apertura = TesoreriaCierre.RegistrarApertura(
            caja.SucursalId,
            caja.Id,
            caja.NroCierreActual + 1,
            fechaApertura,
            saldoInicial,
            observacion,
            currentUser.UserId);

        await cierreRepo.AddAsync(apertura, ct);

        if (saldoInicial > 0)
        {
            await RegistrarMovimientoAsync(
                caja.SucursalId,
                caja.Id,
                fechaApertura,
                TipoOperacionTesoreria.AperturaCaja,
                SentidoMovimientoTesoreria.Ingreso,
                saldoInicial,
                caja.MonedaId,
                1m,
                null,
                "APERTURA_CAJA",
                null,
                observacion,
                ct);
        }

        return apertura;
    }

    public async Task<TesoreriaCierre> CerrarCajaAsync(
        long cajaId,
        DateOnly fechaCierre,
        decimal saldoInformado,
        string? observacion,
        CancellationToken ct = default)
    {
        var caja = await db.CajasCuentasBancarias.FirstOrDefaultAsync(x => x.Id == cajaId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la caja con ID {cajaId}.");

        if (!caja.EstaAbierta)
            throw new InvalidOperationException("La caja no está abierta.");

        var fechaDesde = caja.FechaUltimaApertura ?? fechaCierre;
        var movimientos = await db.TesoreriaMovimientos
            .AsNoTracking()
            .Where(x => x.CajaCuentaId == caja.Id && !x.Anulado && x.Fecha >= fechaDesde && x.Fecha <= fechaCierre)
            .ToListAsync(ct);

        var ingresos = movimientos.Where(x => x.Sentido == SentidoMovimientoTesoreria.Ingreso).Sum(x => x.Importe * x.Cotizacion);
        var egresos = movimientos.Where(x => x.Sentido == SentidoMovimientoTesoreria.Egreso).Sum(x => x.Importe * x.Cotizacion);
        var saldoSistema = caja.SaldoApertura + ingresos - egresos;

        var nroCierre = caja.CerrarArqueo(currentUser.UserId);

        var cierre = TesoreriaCierre.RegistrarCierre(
            caja.SucursalId,
            caja.Id,
            nroCierre,
            fechaCierre,
            saldoInformado,
            saldoSistema,
            ingresos,
            egresos,
            movimientos.Count,
            observacion,
            currentUser.UserId);

        await cierreRepo.AddAsync(cierre, ct);

        if (saldoInformado > 0)
        {
            await RegistrarMovimientoAsync(
                caja.SucursalId,
                caja.Id,
                fechaCierre,
                TipoOperacionTesoreria.CierreCaja,
                SentidoMovimientoTesoreria.Egreso,
                saldoInformado,
                caja.MonedaId,
                1m,
                null,
                "CIERRE_CAJA",
                cierre.Id,
                observacion,
                ct);
        }

        return cierre;
    }

    public async Task<IReadOnlyList<TesoreriaMovimiento>> ObtenerMovimientosAsync(
        long? cajaCuentaId,
        DateOnly? desde,
        DateOnly? hasta,
        TipoOperacionTesoreria? tipoOperacion,
        bool incluirAnulados,
        CancellationToken ct = default)
    {
        var query = db.TesoreriaMovimientos.AsNoTracking();

        if (cajaCuentaId.HasValue)
            query = query.Where(x => x.CajaCuentaId == cajaCuentaId.Value);
        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);
        if (tipoOperacion.HasValue)
            query = query.Where(x => x.TipoOperacion == tipoOperacion.Value);
        if (!incluirAnulados)
            query = query.Where(x => !x.Anulado);

        return await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);
    }

    private async Task ValidarCajaOperableAsync(long cajaCuentaId, CancellationToken ct)
    {
        var caja = await db.CajasCuentasBancarias
            .AsNoTrackingSafe()
            .FirstOrDefaultSafeAsync(x => x.Id == cajaCuentaId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la caja/cuenta ID {cajaCuentaId}.");

        if (!caja.Activa)
            throw new InvalidOperationException("La caja/cuenta está inactiva.");

        if (caja.EsCaja && !caja.EstaAbierta)
            throw new InvalidOperationException("La caja debe estar abierta para registrar movimientos.");
    }
}
