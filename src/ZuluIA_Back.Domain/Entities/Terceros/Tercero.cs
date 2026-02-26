using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Domain.Entities.Terceros;

public class Tercero : AuditableEntity
{
    public string Legajo { get; private set; } = string.Empty;
    public string RazonSocial { get; private set; } = string.Empty;
    public string? NombreFantasia { get; private set; }
    public long TipoDocumentoId { get; private set; }
    public string NroDocumento { get; private set; } = string.Empty;
    public long CondicionIvaId { get; private set; }
    public long? CategoriaId { get; private set; }
    public bool EsCliente { get; private set; }
    public bool EsProveedor { get; private set; }
    public bool EsEmpleado { get; private set; }
    public Domicilio Domicilio { get; private set; } = Domicilio.Vacio();
    public string? NroIngresosBrutos { get; private set; }
    public string? NroMunicipal { get; private set; }
    public string? Telefono { get; private set; }
    public string? Celular { get; private set; }
    public string? Email { get; private set; }
    public string? Web { get; private set; }
    public long? MonedaId { get; private set; }
    public decimal? LimiteCredito { get; private set; }
    public bool Facturable { get; private set; } = true;
    public long? SucursalId { get; private set; }
    public bool Activo { get; private set; } = true;

    private Tercero() { }

    public static Tercero Crear(
        string legajo,
        string razonSocial,
        long tipoDocumentoId,
        string nroDocumento,
        long condicionIvaId,
        bool esCliente,
        bool esProveedor,
        long? sucursalId,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(legajo);
        ArgumentException.ThrowIfNullOrWhiteSpace(razonSocial);
        ArgumentException.ThrowIfNullOrWhiteSpace(nroDocumento);

        var tercero = new Tercero
        {
            Legajo          = legajo.Trim().ToUpperInvariant(),
            RazonSocial     = razonSocial.Trim(),
            TipoDocumentoId = tipoDocumentoId,
            NroDocumento    = nroDocumento.Trim(),
            CondicionIvaId  = condicionIvaId,
            EsCliente       = esCliente,
            EsProveedor     = esProveedor,
            SucursalId      = sucursalId,
            Activo          = true,
            Facturable      = true
        };

        tercero.SetCreated(userId);
        tercero.AddDomainEvent(new TerceroCreadoEvent(tercero.Legajo, esCliente, esProveedor));

        return tercero;
    }

    public void Actualizar(
        string razonSocial,
        string? nombreFantasia,
        string? telefono,
        string? celular,
        string? email,
        string? web,
        Domicilio domicilio,
        decimal? limiteCredito,
        long? userId)
    {
        RazonSocial    = razonSocial.Trim();
        NombreFantasia = nombreFantasia?.Trim();
        Telefono       = telefono?.Trim();
        Celular        = celular?.Trim();
        Email          = email?.Trim().ToLowerInvariant();
        Web            = web?.Trim();
        Domicilio      = domicilio;
        LimiteCredito  = limiteCredito;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
        AddDomainEvent(new TerceroDesactivadoEvent(Id, Legajo));
    }

    public void Activar(long? userId)
    {
        Activo    = true;
        DeletedAt = null;
        SetUpdated(userId);
    }

    public void SetDomicilio(Domicilio domicilio) =>
        Domicilio = domicilio;

    public void SetMoneda(long monedaId) =>
        MonedaId = monedaId;

    public void SetCategoria(long? categoriaId) =>
        CategoriaId = categoriaId;

    public void SetNroIngresosBrutos(string? nro) =>
        NroIngresosBrutos = nro?.Trim();

    private new DateTimeOffset? DeletedAt
    {
        set => base.SetDeleted();
    }
}