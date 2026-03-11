namespace ZuluIA_Back.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el número de un comprobante AFIP.
/// Formato: XXXX-XXXXXXXX (prefijo 4 dígitos - número 8 dígitos)
/// </summary>
public record NumeroComprobante(short Prefijo, long Numero)
{
    public string Formateado => $"{Prefijo:D4}-{Numero:D8}";

    public static NumeroComprobante Crear(short prefijo, long numero)
    {
        if (prefijo <= 0)
            throw new InvalidOperationException(
                "El prefijo debe ser mayor a 0.");
        if (numero <= 0)
            throw new InvalidOperationException(
                "El número de comprobante debe ser mayor a 0.");

        return new NumeroComprobante(prefijo, numero);
    }

    public static NumeroComprobante Parse(string formateado)
    {
        var partes = formateado.Split('-');
        if (partes.Length != 2)
            throw new FormatException(
                $"Formato de número inválido: {formateado}. Esperado: XXXX-XXXXXXXX");

        if (!short.TryParse(partes[0], out var prefijo) ||
            !long.TryParse(partes[1], out var numero))
            throw new FormatException(
                $"No se pudo parsear el número de comprobante: {formateado}");

        return Crear(prefijo, numero);
    }

    public override string ToString() => Formateado;
}