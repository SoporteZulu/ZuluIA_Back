using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Finanzas;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class Cobro : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long TerceroId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1;
    public decimal Total { get; private set; }
    public string? Observacion { get; private set; }
    public EstadoCobro Estado { get; private set; }
    public int? NroCierre { get; private set; }

    // Campos comerciales y operativos
    public long? VendedorId { get; private set; }
    public long? CobradorId { get; private set; }
    public long? ZonaComercialId { get; private set; }
    public long? UsuarioCajeroId { get; private set; }
    public string? VentanillaTurno { get; private set; }
    public TipoCobro TipoCobro { get; private set; } = TipoCobro.Administrativo;
    
    // Observaciones adicionales
    public string? ObservacionInterna { get; private set; }
    
    // Datos del tercero (snapshot)
    public string? TerceroCuit { get; private set; }
    public string? TerceroCondicionIva { get; private set; }
    public string? TerceroDomicilioSnapshot { get; private set; }

    private readonly List<CobroMedio> _medios = [];
    public IReadOnlyCollection<CobroMedio> Medios => _medios.AsReadOnly();

    private Cobro() { }

    public static Cobro Crear(
        long sucursalId,
        long terceroId,
        DateOnly fecha,
        long monedaId,
        decimal cotizacion,
        string? observacion,
        long? vendedorId,
        long? cobradorId,
        long? zonaComercialId,
        long? usuarioCajeroId,
        string? ventanillaTurno,
        TipoCobro tipoCobro,
        string? observacionInterna,
        string? terceroCuit,
        string? terceroCondicionIva,
        string? terceroDomicilioSnapshot,
        long? userId)
    {
        var cobro = new Cobro
        {
            SucursalId                 = sucursalId,
            TerceroId                  = terceroId,
            Fecha                      = fecha,
            MonedaId                   = monedaId,
            Cotizacion                 = cotizacion <= 0 ? 1 : cotizacion,
            Estado                     = EstadoCobro.Activo,
            Observacion                = observacion?.Trim(),
            VendedorId                 = vendedorId,
            CobradorId                 = cobradorId,
            ZonaComercialId            = zonaComercialId,
            UsuarioCajeroId            = usuarioCajeroId,
            VentanillaTurno            = ventanillaTurno?.Trim(),
            TipoCobro                  = tipoCobro,
            ObservacionInterna         = observacionInterna?.Trim(),
            TerceroCuit                = terceroCuit?.Trim(),
            TerceroCondicionIva        = terceroCondicionIva?.Trim(),
            TerceroDomicilioSnapshot   = terceroDomicilioSnapshot?.Trim(),
            Total                      = 0
        };

        cobro.SetCreated(userId);
        return cobro;
    }

    public static Cobro Crear(
        long sucursalId,
        long terceroId,
        DateOnly fecha,
        long monedaId,
        decimal cotizacion,
        string? observacion,
        long? userId)
    {
        return Crear(
            sucursalId,
            terceroId,
            fecha,
            monedaId,
            cotizacion,
            observacion,
            null,
            null,
            null,
            null,
            null,
            TipoCobro.Administrativo,
            null,
            null,
            null,
            null,
            userId);
    }

    public void AgregarMedio(CobroMedio medio)
    {
        if (Estado != EstadoCobro.Activo)
            throw new InvalidOperationException("No se pueden agregar medios a un cobro inactivo.");

        _medios.Add(medio);
        RecalcularTotal();
        AddDomainEvent(new CobroRegistradoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
    }

    private void RecalcularTotal() =>
        Total = _medios.Sum(x => x.Importe * x.Cotizacion);

    public void Anular(long? userId)
    {
        if (Estado == EstadoCobro.Anulado)
            throw new InvalidOperationException("El cobro ya está anulado.");

        Estado = EstadoCobro.Anulado;
        SetDeleted();
        SetUpdated(userId);
        AddDomainEvent(new CobroAnuladoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
    }

    public void AsignarCierre(int nroCierre, long? userId)
    {
        NroCierre = nroCierre;
        SetUpdated(userId);
    }
}
