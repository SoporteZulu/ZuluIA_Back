using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.ValueObjects;

public sealed class NroComprobante : ValueObject
{
    public short Prefijo { get; }
    public long Numero { get; }

    private NroComprobante() { }

    public NroComprobante(short prefijo, long numero)
    {
        if (prefijo <= 0)
            throw new ArgumentException("El prefijo debe ser mayor a 0.", nameof(prefijo));
        if (numero <= 0)
            throw new ArgumentException("El número debe ser mayor a 0.", nameof(numero));

        Prefijo = prefijo;
        Numero  = numero;
    }

    public string Formateado =>
        $"{Prefijo:D4}-{Numero:D8}";

    public override string ToString() => Formateado;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Prefijo;
        yield return Numero;
    }
}