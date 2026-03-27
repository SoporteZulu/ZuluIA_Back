using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

/// <summary>
/// Tipo de entrega/despacho para comprobantes (ej: envío, retiro, correo, transporte).
/// Migrado desde VB6: clsComprobantesTipoEntrega / COMPROBANTESTIPOENTREGA.
/// </summary>
public class TipoEntrega : BaseEntity
{
    public string  Codigo             { get; private set; } = string.Empty;
    public string  Descripcion        { get; private set; } = string.Empty;
    public long?   TipoComprobanteId  { get; private set; }
    public string? Prefijo            { get; private set; }

    private TipoEntrega() { }

    public static TipoEntrega Crear(string codigo, string descripcion,
        long? tipoComprobanteId = null, string? prefijo = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new TipoEntrega
        {
            Codigo            = codigo.Trim().ToUpperInvariant(),
            Descripcion       = descripcion.Trim(),
            TipoComprobanteId = tipoComprobanteId,
            Prefijo           = prefijo?.Trim()
        };
    }

    public void Actualizar(string descripcion, long? tipoComprobanteId, string? prefijo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion       = descripcion.Trim();
        TipoComprobanteId = tipoComprobanteId;
        Prefijo           = prefijo?.Trim();
    }
}
