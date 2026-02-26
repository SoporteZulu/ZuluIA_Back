using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.ValueObjects;

public sealed class Dinero : ValueObject
{
    public decimal Importe { get; }
    public long MonedaId { get; }

    private Dinero() { }

    public Dinero(decimal importe, long monedaId)
    {
        if (monedaId <= 0)
            throw new ArgumentException("MonedaId debe ser mayor a 0.", nameof(monedaId));

        Importe  = importe;
        MonedaId = monedaId;
    }

    public static Dinero Cero(long monedaId) => new(0, monedaId);

    public Dinero Sumar(decimal valor) => new(Importe + valor, MonedaId);
    public Dinero Restar(decimal valor) => new(Importe - valor, MonedaId);
    public Dinero Multiplicar(decimal factor) => new(Importe * factor, MonedaId);

    public override string ToString() => $"{Importe:N2} (Moneda: {MonedaId})";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Importe;
        yield return MonedaId;
    }
}