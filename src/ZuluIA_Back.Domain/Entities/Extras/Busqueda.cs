using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

public class Busqueda : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Modulo { get; private set; } = string.Empty;
    public string FiltrosJson { get; private set; } = "{}";
    public long? UsuarioId { get; private set; }
    public bool EsGlobal { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Busqueda() { }

    public static Busqueda Crear(
        string nombre,
        string modulo,
        string filtrosJson,
        long? usuarioId,
        bool esGlobal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(modulo);

        return new Busqueda
        {
            Nombre      = nombre.Trim(),
            Modulo      = modulo.Trim().ToLowerInvariant(),
            FiltrosJson = filtrosJson,
            UsuarioId   = usuarioId,
            EsGlobal    = esGlobal,
            CreatedAt   = DateTimeOffset.UtcNow,
            UpdatedAt   = DateTimeOffset.UtcNow
        };
    }

    public void Actualizar(string nombre, string filtrosJson, bool esGlobal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        Nombre      = nombre.Trim();
        FiltrosJson = filtrosJson;
        EsGlobal    = esGlobal;
        UpdatedAt   = DateTimeOffset.UtcNow;
    }
}