using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.PuntoVenta;

public class TimbradoFiscal : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long PuntoFacturacionId { get; private set; }
    public string NumeroTimbrado { get; private set; } = string.Empty;
    public DateOnly VigenciaDesde { get; private set; }
    public DateOnly VigenciaHasta { get; private set; }
    public bool Activo { get; private set; } = true;
    public string? Observacion { get; private set; }

    private TimbradoFiscal() { }

    public static TimbradoFiscal Crear(long sucursalId, long puntoFacturacionId, string numeroTimbrado, DateOnly vigenciaDesde, DateOnly vigenciaHasta, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numeroTimbrado);
        if (vigenciaHasta < vigenciaDesde)
            throw new InvalidOperationException("La vigencia hasta no puede ser anterior a la vigencia desde.");

        var timbrado = new TimbradoFiscal
        {
            SucursalId = sucursalId,
            PuntoFacturacionId = puntoFacturacionId,
            NumeroTimbrado = numeroTimbrado.Trim().ToUpperInvariant(),
            VigenciaDesde = vigenciaDesde,
            VigenciaHasta = vigenciaHasta,
            Observacion = observacion?.Trim(),
            Activo = true
        };

        timbrado.SetCreated(userId);
        return timbrado;
    }

    public bool VigentePara(DateOnly fecha) => Activo && fecha >= VigenciaDesde && fecha <= VigenciaHasta;

    public void Actualizar(string numeroTimbrado, DateOnly vigenciaDesde, DateOnly vigenciaHasta, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numeroTimbrado);
        if (vigenciaHasta < vigenciaDesde)
            throw new InvalidOperationException("La vigencia hasta no puede ser anterior a la vigencia desde.");

        NumeroTimbrado = numeroTimbrado.Trim().ToUpperInvariant();
        VigenciaDesde = vigenciaDesde;
        VigenciaHasta = vigenciaHasta;
        Observacion = observacion?.Trim();
        SetUpdated(userId);
    }

    public void Desactivar(string? observacion, long? userId)
    {
        Activo = false;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }

    public bool SuperponeCon(DateOnly vigenciaDesde, DateOnly vigenciaHasta)
        => vigenciaDesde <= VigenciaHasta && vigenciaHasta >= VigenciaDesde;
}
