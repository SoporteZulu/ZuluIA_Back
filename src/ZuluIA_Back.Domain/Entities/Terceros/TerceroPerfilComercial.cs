using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Terceros;

public class TerceroPerfilComercial : AuditableEntity
{
    public long TerceroId { get; private set; }
    public long? ZonaComercialId { get; private set; }
    public string? Rubro { get; private set; }
    public string? Subrubro { get; private set; }
    public string? Sector { get; private set; }
    public string? CondicionCobranza { get; private set; }
    public RiesgoCrediticioComercial RiesgoCrediticio { get; private set; }
    public decimal? SaldoMaximoVigente { get; private set; }
    public string? VigenciaSaldo { get; private set; }
    public DateOnly? VigenciaSaldoDesde { get; private set; }
    public DateOnly? VigenciaSaldoHasta { get; private set; }
    public string? CondicionVenta { get; private set; }
    public string? PlazoCobro { get; private set; }
    public string? FacturadorPorDefecto { get; private set; }
    public decimal? MinimoFacturaMipymes { get; private set; }
    public string? ObservacionComercial { get; private set; }

    private TerceroPerfilComercial() { }

    public static TerceroPerfilComercial Crear(long terceroId, long? userId)
    {
        if (terceroId < 0)
            throw new ArgumentException("El tercero es obligatorio.", nameof(terceroId));

        var perfil = new TerceroPerfilComercial
        {
            TerceroId = terceroId,
            RiesgoCrediticio = RiesgoCrediticioComercial.Normal
        };

        perfil.SetCreated(userId);
        return perfil;
    }

    public void Actualizar(
        long? zonaComercialId,
        string? rubro,
        string? subrubro,
        string? sector,
        string? condicionCobranza,
        RiesgoCrediticioComercial riesgoCrediticio,
        decimal? saldoMaximoVigente,
        string? vigenciaSaldo,
        DateOnly? vigenciaSaldoDesde,
        DateOnly? vigenciaSaldoHasta,
        string? condicionVenta,
        string? plazoCobro,
        string? facturadorPorDefecto,
        decimal? minimoFacturaMipymes,
        string? observacionComercial,
        long? userId)
    {
        if (saldoMaximoVigente.HasValue && saldoMaximoVigente.Value < 0)
            throw new ArgumentException("El saldo máximo vigente no puede ser negativo.", nameof(saldoMaximoVigente));

        if (minimoFacturaMipymes.HasValue && minimoFacturaMipymes.Value < 0)
            throw new ArgumentException("El mínimo de facturas MiPyMES no puede ser negativo.", nameof(minimoFacturaMipymes));

        if (vigenciaSaldoDesde.HasValue && vigenciaSaldoHasta.HasValue && vigenciaSaldoDesde.Value > vigenciaSaldoHasta.Value)
            throw new ArgumentException("La vigencia desde del saldo no puede ser mayor que la vigencia hasta.", nameof(vigenciaSaldoDesde));

        ZonaComercialId = zonaComercialId;
        Rubro = Normalize(rubro);
        Subrubro = Normalize(subrubro);
        Sector = Normalize(sector);
        CondicionCobranza = Normalize(condicionCobranza);
        RiesgoCrediticio = riesgoCrediticio;
        SaldoMaximoVigente = saldoMaximoVigente;
        VigenciaSaldo = Normalize(vigenciaSaldo);
        VigenciaSaldoDesde = vigenciaSaldoDesde;
        VigenciaSaldoHasta = vigenciaSaldoHasta;
        CondicionVenta = Normalize(condicionVenta);
        PlazoCobro = Normalize(plazoCobro);
        FacturadorPorDefecto = Normalize(facturadorPorDefecto);
        MinimoFacturaMipymes = minimoFacturaMipymes;
        ObservacionComercial = Normalize(observacionComercial);
        SetUpdated(userId);
    }

    public void ActualizarCuentaCorriente(
        decimal? limiteSaldo,
        DateOnly? vigenciaLimiteSaldoDesde,
        DateOnly? vigenciaLimiteSaldoHasta,
        long? userId)
    {
        if (limiteSaldo.HasValue && limiteSaldo.Value < 0)
            throw new ArgumentException("El límite de saldo no puede ser negativo.", nameof(limiteSaldo));

        if (vigenciaLimiteSaldoDesde.HasValue && vigenciaLimiteSaldoHasta.HasValue && vigenciaLimiteSaldoDesde.Value > vigenciaLimiteSaldoHasta.Value)
            throw new ArgumentException("La vigencia desde del saldo no puede ser mayor que la vigencia hasta.", nameof(vigenciaLimiteSaldoDesde));

        SaldoMaximoVigente = limiteSaldo;
        VigenciaSaldoDesde = vigenciaLimiteSaldoDesde;
        VigenciaSaldoHasta = vigenciaLimiteSaldoHasta;
        SetUpdated(userId);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
