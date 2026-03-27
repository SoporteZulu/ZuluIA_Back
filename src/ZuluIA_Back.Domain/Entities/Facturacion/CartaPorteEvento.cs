using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class CartaPorteEvento : AuditableEntity
{
    public long CartaPorteId { get; private set; }
    public long? OrdenCargaId { get; private set; }
    public TipoEventoCartaPorte TipoEvento { get; private set; }
    public EstadoCartaPorte? EstadoAnterior { get; private set; }
    public EstadoCartaPorte EstadoNuevo { get; private set; }
    public DateOnly FechaEvento { get; private set; }
    public string? Mensaje { get; private set; }
    public string? NroCtg { get; private set; }
    public int IntentoCtg { get; private set; }

    private CartaPorteEvento() { }

    public static CartaPorteEvento Registrar(
        long cartaPorteId,
        long? ordenCargaId,
        TipoEventoCartaPorte tipoEvento,
        EstadoCartaPorte? estadoAnterior,
        EstadoCartaPorte estadoNuevo,
        DateOnly fechaEvento,
        string? mensaje,
        string? nroCtg,
        int intentoCtg,
        long? userId)
    {
        var evento = new CartaPorteEvento
        {
            CartaPorteId = cartaPorteId,
            OrdenCargaId = ordenCargaId,
            TipoEvento = tipoEvento,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            FechaEvento = fechaEvento,
            Mensaje = mensaje?.Trim(),
            NroCtg = nroCtg?.Trim(),
            IntentoCtg = intentoCtg
        };

        evento.SetCreated(userId);
        return evento;
    }
}
