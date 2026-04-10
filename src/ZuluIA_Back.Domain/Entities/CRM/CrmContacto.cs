using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

public class CrmContacto : AuditableEntity
{
    public long ClienteId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public string? Cargo { get; private set; }
    public string? Email { get; private set; }
    public string? Telefono { get; private set; }
    public string CanalPreferido { get; private set; } = string.Empty;
    public string EstadoContacto { get; private set; } = string.Empty;
    public string? Notas { get; private set; }
    public bool Activo { get; private set; } = true;

    private CrmContacto() { }

    public static CrmContacto Crear(
        long clienteId,
        string nombre,
        string apellido,
        string? cargo,
        string? email,
        string? telefono,
        string canalPreferido,
        string estadoContacto,
        string? notas,
        long? userId)
    {
        if (clienteId <= 0)
            throw new ArgumentException("El contacto CRM requiere un cliente válido.", nameof(clienteId));

        var entity = new CrmContacto
        {
            ClienteId = clienteId,
            Nombre = NormalizeRequired(nombre, nameof(nombre)),
            Apellido = NormalizeRequired(apellido, nameof(apellido)),
            Cargo = NormalizeOptional(cargo),
            Email = NormalizeEmail(email),
            Telefono = NormalizeOptional(telefono),
            CanalPreferido = NormalizeRequired(canalPreferido, nameof(canalPreferido)),
            EstadoContacto = NormalizeRequired(estadoContacto, nameof(estadoContacto)),
            Notas = NormalizeOptional(notas),
            Activo = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(
        long clienteId,
        string nombre,
        string apellido,
        string? cargo,
        string? email,
        string? telefono,
        string canalPreferido,
        string estadoContacto,
        string? notas,
        long? userId)
    {
        if (clienteId <= 0)
            throw new ArgumentException("El contacto CRM requiere un cliente válido.", nameof(clienteId));

        ClienteId = clienteId;
        Nombre = NormalizeRequired(nombre, nameof(nombre));
        Apellido = NormalizeRequired(apellido, nameof(apellido));
        Cargo = NormalizeOptional(cargo);
        Email = NormalizeEmail(email);
        Telefono = NormalizeOptional(telefono);
        CanalPreferido = NormalizeRequired(canalPreferido, nameof(canalPreferido));
        EstadoContacto = NormalizeRequired(estadoContacto, nameof(estadoContacto));
        Notas = NormalizeOptional(notas);
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeEmail(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
