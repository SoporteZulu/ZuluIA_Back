using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Domain.Entities.Sucursales;

public class Sucursal : AuditableEntity
{
    public string RazonSocial { get; private set; } = string.Empty;
    public string? NombreFantasia { get; private set; }
    public string Cuit { get; private set; } = string.Empty;
    public string? NroIngresosBrutos { get; private set; }
    public long CondicionIvaId { get; private set; }
    public long MonedaId { get; private set; }
    public long PaisId { get; private set; }
    public Domicilio Domicilio { get; private set; } = Domicilio.Vacio();
    public string? Telefono { get; private set; }
    public string? Email { get; private set; }
    public string? Web { get; private set; }
    public string? Cbu { get; private set; }
    public string? AliasCbu { get; private set; }
    public string? Cai { get; private set; }
    public short PuertoAfip { get; private set; } = 443;
    public bool CasaMatriz { get; private set; }
    public bool Activa { get; private set; } = true;

    private Sucursal() { }

    public static Sucursal Crear(
        string razonSocial,
        string cuit,
        long condicionIvaId,
        long monedaId,
        long paisId,
        bool casaMatriz,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(razonSocial);
        ArgumentException.ThrowIfNullOrWhiteSpace(cuit);

        var sucursal = new Sucursal
        {
            RazonSocial    = razonSocial.Trim(),
            Cuit           = cuit.Trim(),
            CondicionIvaId = condicionIvaId,
            MonedaId       = monedaId,
            PaisId         = paisId,
            CasaMatriz     = casaMatriz,
            Activa         = true
        };

        sucursal.SetCreated(userId);
        return sucursal;
    }

    public void Actualizar(
        string razonSocial,
        string? nombreFantasia,
        string cuit,
        string? nroIngresosBrutos,
        long condicionIvaId,
        long monedaId,
        long paisId,
        Domicilio domicilio,
        string? telefono,
        string? email,
        string? web,
        string? cbu,
        string? aliasCbu,
        string? cai,
        short puertoAfip,
        bool casaMatriz,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(razonSocial);
        ArgumentException.ThrowIfNullOrWhiteSpace(cuit);

        RazonSocial       = razonSocial.Trim();
        NombreFantasia    = nombreFantasia?.Trim();
        Cuit              = cuit.Trim();
        NroIngresosBrutos = nroIngresosBrutos?.Trim();
        CondicionIvaId    = condicionIvaId;
        MonedaId          = monedaId;
        PaisId            = paisId;
        Domicilio         = domicilio;
        Telefono          = telefono?.Trim();
        Email             = email?.Trim().ToLowerInvariant();
        Web               = web?.Trim();
        Cbu               = cbu?.Trim();
        AliasCbu          = aliasCbu?.Trim();
        Cai               = cai?.Trim();
        PuertoAfip        = puertoAfip;
        CasaMatriz        = casaMatriz;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activa = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activa = true;
        base.SetDeleted(); // reset DeletedAt
        SetUpdated(userId);
    }
}