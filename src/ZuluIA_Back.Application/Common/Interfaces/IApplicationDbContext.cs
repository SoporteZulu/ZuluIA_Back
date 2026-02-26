using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Pais> Paises { get; }
    DbSet<Provincia> Provincias { get; }
    DbSet<Localidad> Localidades { get; }
    DbSet<Barrio> Barrios { get; }
    DbSet<Tercero> Terceros { get; }
    DbSet<Item> Items { get; }
    DbSet<StockItem> Stock { get; }
    DbSet<MovimientoStock> MovimientosStock { get; }
    DbSet<Comprobante> Comprobantes { get; }
    DbSet<ComprobanteItem> ComprobantesItems { get; }
    DbSet<Cobro> Cobros { get; }
    DbSet<CobroMedio> CobrosMedios { get; }
    DbSet<Pago> Pagos { get; }
    DbSet<PagoMedio> PagosMedios { get; }
    DbSet<Asiento> Asientos { get; }
    DbSet<AsientoLinea> AsientosLineas { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}