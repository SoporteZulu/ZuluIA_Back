using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Items;

public class AtributoComercial : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public TipoDatoAtributoComercial TipoDato { get; private set; }
    public bool Activo { get; private set; } = true;

    private AtributoComercial() { }

    public static AtributoComercial Crear(string codigo, string descripcion, TipoDatoAtributoComercial tipoDato, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var entity = new AtributoComercial
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            TipoDato = tipoDato,
            Activo = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(string descripcion, TipoDatoAtributoComercial tipoDato, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
        TipoDato = tipoDato;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void ValidarValor(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var valorNormalizado = valor.Trim();
        switch (TipoDato)
        {
            case TipoDatoAtributoComercial.Texto:
                return;
            case TipoDatoAtributoComercial.Numero:
                if (!decimal.TryParse(valorNormalizado, out _))
                    throw new InvalidOperationException("El valor informado no es numérico para el atributo comercial.");
                return;
            case TipoDatoAtributoComercial.Fecha:
                if (!DateOnly.TryParse(valorNormalizado, out _))
                    throw new InvalidOperationException("El valor informado no es una fecha válida para el atributo comercial.");
                return;
            case TipoDatoAtributoComercial.Booleano:
                var esBooleano = bool.TryParse(valorNormalizado, out _)
                    || valorNormalizado.Equals("SI", StringComparison.OrdinalIgnoreCase)
                    || valorNormalizado.Equals("NO", StringComparison.OrdinalIgnoreCase)
                    || valorNormalizado is "1" or "0";

                if (!esBooleano)
                    throw new InvalidOperationException("El valor informado no es booleano para el atributo comercial.");
                return;
        }
    }
}
