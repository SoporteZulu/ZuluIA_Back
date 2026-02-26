using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.ValueObjects;

public sealed class Domicilio : ValueObject
{
    public string? Calle { get; }
    public string? Nro { get; }
    public string? Piso { get; }
    public string? Dpto { get; }
    public string? CodigoPostal { get; }
    public long? LocalidadId { get; }
    public long? BarrioId { get; }

    private Domicilio() { }

    public Domicilio(
        string? calle,
        string? nro,
        string? piso,
        string? dpto,
        string? codigoPostal,
        long? localidadId,
        long? barrioId)
    {
        Calle        = calle;
        Nro          = nro;
        Piso         = piso;
        Dpto         = dpto;
        CodigoPostal = codigoPostal;
        LocalidadId  = localidadId;
        BarrioId     = barrioId;
    }

    public static Domicilio Vacio() => new(null, null, null, null, null, null, null);

    public string Completo =>
        string.Join(" ", new[] { Calle, Nro, Piso, Dpto }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Calle;
        yield return Nro;
        yield return Piso;
        yield return Dpto;
        yield return CodigoPostal;
        yield return LocalidadId;
        yield return BarrioId;
    }
}