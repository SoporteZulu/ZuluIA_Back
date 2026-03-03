using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Usuarios;

/// <summary>
/// Parámetro personalizado de configuración por usuario.
/// </summary>
public class ParametroUsuario : BaseEntity
{
    public long UsuarioId { get; private set; }
    public string Clave { get; private set; } = string.Empty;
    public string? Valor { get; private set; }

    private ParametroUsuario() { }

    public static ParametroUsuario Crear(long usuarioId, string clave, string? valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clave);

        return new ParametroUsuario
        {
            UsuarioId = usuarioId,
            Clave     = clave.Trim().ToUpperInvariant(),
            Valor     = valor
        };
    }

    public void SetValor(string? valor) => Valor = valor;
}