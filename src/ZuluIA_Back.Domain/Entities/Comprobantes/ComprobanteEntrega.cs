using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

/// <summary>
/// Datos de entrega para un comprobante (dirección de despacho/envío).
/// Permite registrar una dirección de entrega diferente al domicilio del tercero.
/// Migrado desde VB6: clsComprobantesEntrega / COMPROBANTESENTREGA.
/// </summary>
public class ComprobanteEntrega : BaseEntity
{
    public long      ComprobanteId   { get; private set; }
    public DateOnly  Fecha           { get; private set; }
    public string?   RazonSocial     { get; private set; }
    public string?   Domicilio       { get; private set; }
    public long?     LocalidadId     { get; private set; }
    public long?     ProvinciaId     { get; private set; }
    public long?     PaisId          { get; private set; }
    public string?   CodigoPostal    { get; private set; }
    public string?   Telefono1       { get; private set; }
    public string?   Telefono2       { get; private set; }
    public string?   Celular         { get; private set; }
    public string?   Email           { get; private set; }
    public string?   Observacion     { get; private set; }
    public long?     TipoEntregaId   { get; private set; }
    public long?     TransportistaId { get; private set; }
    public long?     ZonaId          { get; private set; }

    private ComprobanteEntrega() { }

    public static ComprobanteEntrega Crear(long comprobanteId, DateOnly fecha,
        string? razonSocial = null, string? domicilio = null,
        long? localidadId = null, long? provinciaId = null, long? paisId = null,
        string? codigoPostal = null, string? telefono1 = null, string? telefono2 = null,
        string? celular = null, string? email = null, string? observacion = null,
        long? tipoEntregaId = null, long? transportistaId = null, long? zonaId = null)
    {
        if (comprobanteId <= 0) throw new ArgumentException("El comprobante es requerido.");

        return new ComprobanteEntrega
        {
            ComprobanteId   = comprobanteId,
            Fecha           = fecha,
            RazonSocial     = razonSocial?.Trim(),
            Domicilio       = domicilio?.Trim(),
            LocalidadId     = localidadId,
            ProvinciaId     = provinciaId,
            PaisId          = paisId,
            CodigoPostal    = codigoPostal?.Trim(),
            Telefono1       = telefono1?.Trim(),
            Telefono2       = telefono2?.Trim(),
            Celular         = celular?.Trim(),
            Email           = email?.Trim().ToLowerInvariant(),
            Observacion     = observacion?.Trim(),
            TipoEntregaId   = tipoEntregaId,
            TransportistaId = transportistaId,
            ZonaId          = zonaId
        };
    }

    public void Actualizar(string? razonSocial, string? domicilio,
        long? localidadId, long? provinciaId, long? paisId,
        string? codigoPostal, string? telefono1, string? telefono2,
        string? celular, string? email, string? observacion,
        long? tipoEntregaId, long? transportistaId, long? zonaId)
    {
        RazonSocial     = razonSocial?.Trim();
        Domicilio       = domicilio?.Trim();
        LocalidadId     = localidadId;
        ProvinciaId     = provinciaId;
        PaisId          = paisId;
        CodigoPostal    = codigoPostal?.Trim();
        Telefono1       = telefono1?.Trim();
        Telefono2       = telefono2?.Trim();
        Celular         = celular?.Trim();
        Email           = email?.Trim().ToLowerInvariant();
        Observacion     = observacion?.Trim();
        TipoEntregaId   = tipoEntregaId;
        TransportistaId = transportistaId;
        ZonaId          = zonaId;
    }
}
