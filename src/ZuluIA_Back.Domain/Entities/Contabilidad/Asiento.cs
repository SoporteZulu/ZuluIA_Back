using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Contabilidad;

public class Asiento : AuditableEntity
{
    public long EjercicioId { get; private set; }
    public long SucursalId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public long Numero { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public string? OrigenTabla { get; private set; }
    public long? OrigenId { get; private set; }
    public EstadoAsiento Estado { get; private set; }

    private readonly List<AsientoLinea> _lineas = [];
    public IReadOnlyCollection<AsientoLinea> Lineas => _lineas.AsReadOnly();

    private Asiento() { }

    public static Asiento Crear(
        long ejercicioId,
        long sucursalId,
        DateOnly fecha,
        long numero,
        string descripcion,
        string? origenTabla,
        long? origenId,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var asiento = new Asiento
        {
            EjercicioId = ejercicioId,
            SucursalId  = sucursalId,
            Fecha       = fecha,
            Numero      = numero,
            Descripcion = descripcion.Trim(),
            OrigenTabla = origenTabla,
            OrigenId    = origenId,
            Estado      = EstadoAsiento.Borrador
        };

        asiento.SetCreated(userId);
        return asiento;
    }

    public void AgregarLinea(AsientoLinea linea)
    {
        if (Estado != EstadoAsiento.Borrador)
            throw new InvalidOperationException("No se pueden agregar líneas a un asiento contabilizado.");

        _lineas.Add(linea);
    }

    public void Contabilizar(long? userId)
    {
        if (Estado != EstadoAsiento.Borrador)
            throw new InvalidOperationException("Solo se pueden contabilizar asientos en estado Borrador.");

        var totalDebe = _lineas.Sum(l => l.Debe);
        var totalHaber = _lineas.Sum(l => l.Haber);

        if (Math.Round(totalDebe, 2) != Math.Round(totalHaber, 2))
            throw new InvalidOperationException(
                $"El asiento no balancea. Debe: {totalDebe:N2} | Haber: {totalHaber:N2}");

        Estado = EstadoAsiento.Contabilizado;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoAsiento.Anulado)
            throw new InvalidOperationException("El asiento ya está anulado.");

        Estado = EstadoAsiento.Anulado;
        SetUpdated(userId);
    }

    public decimal TotalDebe => _lineas.Sum(l => l.Debe);
    public decimal TotalHaber => _lineas.Sum(l => l.Haber);
    public bool Balancea => Math.Round(TotalDebe, 2) == Math.Round(TotalHaber, 2);
}