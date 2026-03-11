using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Mapea la tabla "config" — clave/valor de parámetros del sistema.
/// </summary>
public class ConfiguracionSistema : BaseEntity
{
    public string Campo { get; private set; } = string.Empty;
    public string? Valor { get; private set; }
    public short TipoDato { get; private set; } = 1;
    public string? Descripcion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private ConfiguracionSistema() { }

    public static ConfiguracionSistema Crear(string campo, string? valor, short tipoDato, string? descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(campo);
        return new ConfiguracionSistema
        {
            Campo       = campo.Trim().ToUpperInvariant(),
            Valor       = valor,
            TipoDato    = tipoDato,
            Descripcion = descripcion,
            CreatedAt   = DateTimeOffset.UtcNow,
            UpdatedAt   = DateTimeOffset.UtcNow
        };
    }

    public void SetValor(string? valor)
    {
        Valor     = valor;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}