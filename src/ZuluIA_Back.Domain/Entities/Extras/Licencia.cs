using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

/// <summary>
/// Registro de licencia del sistema (habilitación, vencimiento, módulos activos).
/// Migrado desde VB6: Licencia_Registrar.
/// </summary>
public class Licencia : AuditableEntity
{
    public string   Clave           { get; private set; } = string.Empty;
    public string   NombreEmpresa   { get; private set; } = string.Empty;
    public string   Ruc             { get; private set; } = string.Empty;
    public DateOnly FechaActivacion { get; private set; }
    public DateOnly? FechaVencimiento { get; private set; }
    public string   Modulos         { get; private set; } = string.Empty;
    public bool     Activa          { get; private set; } = true;

    private Licencia() { }

    public static Licencia Registrar(
        string clave, string nombreEmpresa, string ruc,
        DateOnly fechaActivacion, DateOnly? fechaVencimiento,
        string modulos, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clave);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreEmpresa);
        var l = new Licencia
        {
            Clave            = clave.Trim(),
            NombreEmpresa    = nombreEmpresa.Trim(),
            Ruc              = ruc.Trim(),
            FechaActivacion  = fechaActivacion,
            FechaVencimiento = fechaVencimiento,
            Modulos          = modulos.Trim(),
            Activa           = true
        };
        l.SetCreated(userId);
        return l;
    }

    public void Renovar(DateOnly nuevaFechaVencimiento, string modulos, long? userId)
    {
        FechaVencimiento = nuevaFechaVencimiento;
        Modulos          = modulos.Trim();
        Activa           = true;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }
}
