using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.ValueObjects;

public sealed class Domicilio : ValueObject
{
    public string? Calle { get; }
    public string? Nro { get; }
    public string? Piso { get; }
    public string? Dpto { get; }
    public string? CodigoPostal { get; }
    public long? ProvinciaId { get; }
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
        long? barrioId,
        long? provinciaId = null)
    {
        Calle        = calle;
        Nro          = nro;
        Piso         = piso;
        Dpto         = dpto;
        CodigoPostal = codigoPostal;
        ProvinciaId  = provinciaId;
        LocalidadId  = localidadId;
        BarrioId     = barrioId;
    }

    public static Domicilio Crear(
    string? calle,
    string? nro,
    string? piso,
    string? dpto,
    string? codigoPostal,
    long? localidadId,
    long? barrioId,
    long? provinciaId = null)
    {
        return new Domicilio(
            calle,
            nro,
            piso,
            dpto,
            codigoPostal,
            localidadId,
            barrioId,
            provinciaId
        );
    }

    public static Domicilio Vacio() => new(null, null, null, null, null, null, null, null);

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
        yield return ProvinciaId;
        yield return LocalidadId;
        yield return BarrioId;
    }
}