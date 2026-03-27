using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

public class TerceroVentanaCobranza : AuditableEntity
{
    public long TerceroId { get; private set; }
    public string Dia { get; private set; } = string.Empty;
    public string? Franja { get; private set; }
    public string? Canal { get; private set; }
    public string? Responsable { get; private set; }
    public bool Principal { get; private set; }
    public int Orden { get; private set; }

    private TerceroVentanaCobranza() { }

    public static TerceroVentanaCobranza Crear(
        long terceroId,
        string dia,
        string? franja,
        string? canal,
        string? responsable,
        bool principal,
        int orden,
        long? userId)
    {
        if (terceroId <= 0)
            throw new ArgumentException("El tercero es obligatorio.", nameof(terceroId));

        ArgumentException.ThrowIfNullOrWhiteSpace(dia);
        if (orden < 0)
            throw new ArgumentException("El orden de la ventana de cobranza no es válido.", nameof(orden));

        var ventana = new TerceroVentanaCobranza
        {
            TerceroId = terceroId,
            Dia = dia.Trim(),
            Franja = Normalize(franja),
            Canal = Normalize(canal),
            Responsable = Normalize(responsable),
            Principal = principal,
            Orden = orden
        };

        ventana.SetCreated(userId);
        return ventana;
    }

    public void Actualizar(
        string dia,
        string? franja,
        string? canal,
        string? responsable,
        bool principal,
        int orden,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dia);
        if (orden < 0)
            throw new ArgumentException("El orden de la ventana de cobranza no es válido.", nameof(orden));

        Dia = dia.Trim();
        Franja = Normalize(franja);
        Canal = Normalize(canal);
        Responsable = Normalize(responsable);
        Principal = principal;
        Orden = orden;
        SetUpdated(userId);
    }

    public void MarcarComoEliminada(long? userId)
    {
        SetDeleted();
        SetUpdated(userId);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
