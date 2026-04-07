using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

public class MotivoDebito : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool EsFiscal { get; private set; }
    public bool RequiereDocumentoOrigen { get; private set; } = true;
    public bool AfectaCuentaCorriente { get; private set; } = true;
    public bool Activo { get; private set; } = true;

    private MotivoDebito() { }

    public static MotivoDebito Crear(
        string codigo,
        string descripcion,
        bool esFiscal,
        bool requiereDocumentoOrigen,
        bool afectaCuentaCorriente)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo, nameof(codigo));
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion, nameof(descripcion));

        return new MotivoDebito
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            EsFiscal = esFiscal,
            RequiereDocumentoOrigen = requiereDocumentoOrigen,
            AfectaCuentaCorriente = afectaCuentaCorriente,
            Activo = true
        };
    }

    public void Actualizar(
        string descripcion,
        bool esFiscal,
        bool requiereDocumentoOrigen,
        bool afectaCuentaCorriente)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion, nameof(descripcion));

        Descripcion = descripcion.Trim();
        EsFiscal = esFiscal;
        RequiereDocumentoOrigen = requiereDocumentoOrigen;
        AfectaCuentaCorriente = afectaCuentaCorriente;
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;
}
