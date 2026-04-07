using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

public class AspectoDiagnostico : AuditableEntity
{
    public long RegionId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Peso { get; private set; }
    public bool Activo { get; private set; } = true;

    private AspectoDiagnostico() { }

    public static AspectoDiagnostico Crear(long regionId, string codigo, string descripcion, decimal peso, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (peso <= 0)
            throw new InvalidOperationException("El peso del aspecto debe ser mayor a 0.");

        var aspecto = new AspectoDiagnostico
        {
            RegionId = regionId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            Peso = peso,
            Activo = true
        };

        aspecto.SetCreated(userId);
        return aspecto;
    }
}
