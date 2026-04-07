using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Franquicias;

/// <summary>
/// Valor de una variable del plan de trabajo asignado a un usuario específico de franquicia.
/// Migrado desde VB6: FRA_VARIABLESXUSUARIOS.
/// </summary>
public class FranquiciaVariableXUsuario : AuditableEntity
{
    public long    PlanTrabajoId { get; private set; }
    public long    UsuarioId     { get; private set; }
    public long    VariableId    { get; private set; }
    public string  Valor         { get; private set; } = string.Empty;

    private FranquiciaVariableXUsuario() { }

    public static FranquiciaVariableXUsuario Crear(long planTrabajoId, long usuarioId, long variableId, string valor, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);
        var v = new FranquiciaVariableXUsuario
        {
            PlanTrabajoId = planTrabajoId,
            UsuarioId     = usuarioId,
            VariableId    = variableId,
            Valor         = valor.Trim()
        };
        v.SetCreated(userId);
        return v;
    }

    public void ActualizarValor(string valor, long? userId) { Valor = valor.Trim(); SetUpdated(userId); }
}
