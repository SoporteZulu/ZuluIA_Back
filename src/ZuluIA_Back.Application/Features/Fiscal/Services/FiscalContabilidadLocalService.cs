using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Fiscal;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Fiscal.Services;

public class FiscalContabilidadLocalService(
    IApplicationDbContext db,
    IRepository<CierrePeriodoContable> cierreRepo,
    IRepository<ReorganizacionAsientos> reorganizacionRepo,
    IRepository<LibroViajanteRegistro> libroRepo,
    IRepository<RentasBsAsRegistro> rentasRepo,
    IRepository<HechaukaRegistro> hechaukaRepo,
    IRepository<LiquidacionPrimariaGrano> lpgRepo,
    IRepository<SalidaRegulatoria> salidaRepo,
    ICurrentUserService currentUser)
{
    public async Task<CierrePeriodoContable> CerrarPeriodoContableAsync(long ejercicioId, long? sucursalId, DateOnly desde, DateOnly hasta, string? observacion, CancellationToken ct)
    {
        var ejercicio = await db.Ejercicios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ejercicioId, ct)
            ?? throw new InvalidOperationException($"No se encontró el ejercicio ID {ejercicioId}.");

        if (!ejercicio.ContienesFecha(desde) || !ejercicio.ContienesFecha(hasta))
            throw new InvalidOperationException("El rango del cierre debe pertenecer al ejercicio indicado.");

        var superpuesto = await db.CierresPeriodoContable.AsNoTracking()
            .AnyAsync(x => x.EjercicioId == ejercicioId
                && x.SucursalId == sucursalId
                && x.Desde <= hasta
                && x.Hasta >= desde, ct);

        if (superpuesto)
            throw new InvalidOperationException("Ya existe un cierre contable superpuesto para el rango indicado.");

        var cierre = CierrePeriodoContable.Crear(ejercicioId, sucursalId, desde, hasta, observacion, currentUser.UserId);
        await cierreRepo.AddAsync(cierre, ct);

        if (sucursalId.HasValue && desde.Day == 1 && hasta == new DateOnly(hasta.Year, hasta.Month, DateTime.DaysInMonth(hasta.Year, hasta.Month)))
        {
            var periodo = new DateOnly(desde.Year, desde.Month, 1);
            var periodoIva = await db.PeriodosIva.FirstOrDefaultAsync(x => x.EjercicioId == ejercicioId && x.SucursalId == sucursalId.Value && x.Periodo == periodo, ct);
            if (periodoIva is null)
            {
                periodoIva = PeriodoIva.Crear(ejercicioId, sucursalId.Value, periodo);
                await db.PeriodosIva.AddAsync(periodoIva, ct);
            }

            if (!periodoIva.Cerrado)
                periodoIva.Cerrar();
        }

        return cierre;
    }

    public async Task<ReorganizacionAsientos> ReorganizarAsientosAsync(long ejercicioId, long? sucursalId, DateOnly desde, DateOnly hasta, string? observacion, CancellationToken ct)
    {
        var asientos = await db.Asientos
            .Where(x => x.EjercicioId == ejercicioId
                && (!sucursalId.HasValue || x.SucursalId == sucursalId.Value)
                && x.Fecha >= desde
                && x.Fecha <= hasta)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        long numero = 1;
        foreach (var asiento in asientos)
            asiento.Renumerar(numero++, currentUser.UserId);

        var registro = ReorganizacionAsientos.Registrar(ejercicioId, sucursalId, desde, hasta, asientos.Count, observacion, currentUser.UserId);
        await reorganizacionRepo.AddAsync(registro, ct);
        return registro;
    }

    public async Task<IReadOnlyList<LibroViajanteRegistro>> GenerarLibroViajantesAsync(long sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var terceros = await db.Terceros.AsNoTracking().ToDictionaryAsync(x => x.Id, ct);
        var comprobantes = await db.Comprobantes.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Fecha >= desde && x.Fecha <= hasta)
            .ToListAsync(ct);

        var registros = comprobantes
            .Where(x => terceros.ContainsKey(x.TerceroId))
            .GroupBy(x => terceros[x.TerceroId].VendedorId)
            .Select(g => LibroViajanteRegistro.Crear(
                sucursalId,
                desde,
                hasta,
                g.Key,
                g.Sum(x => x.Total),
                g.Sum(x => x.Total * (terceros[x.TerceroId].PctComisionVendedor / 100m)),
                currentUser.UserId))
            .ToList();

        foreach (var registro in registros)
            await libroRepo.AddAsync(registro, ct);

        return registros;
    }

    public async Task<RentasBsAsRegistro> GenerarRentasBsAsAsync(long sucursalId, DateOnly desde, DateOnly hasta, string? observacion, CancellationToken ct)
    {
        var comprobantes = await db.Comprobantes.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Fecha >= desde && x.Fecha <= hasta)
            .ToListAsync(ct);

        var registro = RentasBsAsRegistro.Crear(
            sucursalId,
            desde,
            hasta,
            comprobantes.Sum(x => x.Percepciones),
            comprobantes.Sum(x => x.Retenciones),
            observacion,
            currentUser.UserId);

        await rentasRepo.AddAsync(registro, ct);
        return registro;
    }

    public async Task<HechaukaRegistro> GenerarHechaukaAsync(long sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var comprobantes = await db.Comprobantes.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Fecha >= desde && x.Fecha <= hasta)
            .ToListAsync(ct);

        var registro = HechaukaRegistro.Crear(
            sucursalId,
            desde,
            hasta,
            comprobantes.Sum(x => x.NetoGravado),
            comprobantes.Sum(x => x.IvaRi + x.IvaRni),
            comprobantes.Sum(x => x.Total),
            currentUser.UserId);

        await hechaukaRepo.AddAsync(registro, ct);
        return registro;
    }

    public async Task<LiquidacionPrimariaGrano> RegistrarLiquidacionPrimariaGranoAsync(long sucursalId, DateOnly fecha, string numeroLiquidacion, string producto, decimal cantidad, decimal precioUnitario, string? observacion, CancellationToken ct)
    {
        var existe = await db.LiquidacionesPrimariasGranos.AsNoTracking().AnyAsync(x => x.NumeroLiquidacion == numeroLiquidacion.Trim().ToUpperInvariant(), ct);
        if (existe)
            throw new InvalidOperationException($"Ya existe una liquidación primaria con número '{numeroLiquidacion}'.");

        var liquidacion = LiquidacionPrimariaGrano.Crear(sucursalId, fecha, numeroLiquidacion, producto, cantidad, precioUnitario, observacion, currentUser.UserId);
        await lpgRepo.AddAsync(liquidacion, ct);
        return liquidacion;
    }

    public async Task<SalidaRegulatoria> GenerarSalidaRegulatoriaAsync(TipoSalidaRegulatoria tipo, long sucursalId, DateOnly desde, DateOnly hasta, string nombreArchivo, CancellationToken ct)
    {
        var contenido = tipo switch
        {
            TipoSalidaRegulatoria.LibroViajantes => await GenerarContenidoLibroViajantesAsync(sucursalId, desde, hasta, ct),
            TipoSalidaRegulatoria.RentasBsAs => await GenerarContenidoRentasBsAsAsync(sucursalId, desde, hasta, ct),
            TipoSalidaRegulatoria.Hechauka => await GenerarContenidoHechaukaAsync(sucursalId, desde, hasta, ct),
            _ => $"TIPO;{tipo}\nSUCURSAL;{sucursalId}\nDESDE;{desde:yyyy-MM-dd}\nHASTA;{hasta:yyyy-MM-dd}"
        };

        var salida = SalidaRegulatoria.Crear(tipo, sucursalId, desde, hasta, nombreArchivo, contenido, currentUser.UserId);
        await salidaRepo.AddAsync(salida, ct);
        return salida;
    }

    private async Task<string> GenerarContenidoLibroViajantesAsync(long sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var registros = await db.LibrosViajantesRegistros.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Desde == desde && x.Hasta == hasta)
            .OrderBy(x => x.VendedorId)
            .ToListAsync(ct);

        return string.Join("\n", new[] { "VENDEDOR_ID;TOTAL_VENTAS;TOTAL_COMISION" }
            .Concat(registros.Select(x => $"{x.VendedorId};{x.TotalVentas:0.00};{x.TotalComision:0.00}")));
    }

    private async Task<string> GenerarContenidoRentasBsAsAsync(long sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var registro = await db.RentasBsAsRegistros.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Desde == desde && x.Hasta == hasta)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        return registro is null
            ? "SIN_DATOS"
            : $"TOTAL_PERCEPCIONES;{registro.TotalPercepciones:0.00}\nTOTAL_RETENCIONES;{registro.TotalRetenciones:0.00}";
    }

    private async Task<string> GenerarContenidoHechaukaAsync(long sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var registro = await db.HechaukaRegistros.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Desde == desde && x.Hasta == hasta)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        return registro is null
            ? "SIN_DATOS"
            : $"NETO_GRAVADO;{registro.TotalNetoGravado:0.00}\nIVA;{registro.TotalIva:0.00}\nTOTAL_COMPROBANTES;{registro.TotalComprobantes:0.00}";
    }
}
