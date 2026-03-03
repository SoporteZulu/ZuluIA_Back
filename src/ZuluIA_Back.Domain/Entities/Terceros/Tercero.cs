using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Events.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Domain.Entities.Terceros;

public class Tercero : AuditableEntity
{
    // ─── Identificación ───────────────────────────────────────────────────────
    public string Legajo { get; private set; } = string.Empty;
    public string RazonSocial { get; private set; } = string.Empty;
    public string? NombreFantasia { get; private set; }

    // ─── Documento e IVA ──────────────────────────────────────────────────────
    public long TipoDocumentoId { get; private set; }
    public string NroDocumento { get; private set; } = string.Empty;
    public long CondicionIvaId { get; private set; }

    // ─── Clasificación ────────────────────────────────────────────────────────
    public long? CategoriaId { get; private set; }
    public bool EsCliente { get; private set; }
    public bool EsProveedor { get; private set; }
    public bool EsEmpleado { get; private set; }

    // ─── Domicilio (Value Object) ─────────────────────────────────────────────
    public Domicilio Domicilio { get; private set; } = Domicilio.Vacio();

    // ─── Datos fiscales ───────────────────────────────────────────────────────
    public string? NroIngresosBrutos { get; private set; }
    public string? NroMunicipal { get; private set; }

    // ─── Contacto ─────────────────────────────────────────────────────────────
    public string? Telefono { get; private set; }
    public string? Celular { get; private set; }
    public string? Email { get; private set; }
    public string? Web { get; private set; }

    // ─── Comercial ────────────────────────────────────────────────────────────
    public long? MonedaId { get; private set; }
    public decimal? LimiteCredito { get; private set; }
    public bool Facturable { get; private set; } = true;
    public long? SucursalId { get; private set; }
    public string? Observacion { get; private set; }

    // ─── Cobrador/Vendedor ────────────────────────────────────────────────────
    public long? CobradorId { get; private set; }
    public decimal PctComisionCobrador { get; private set; }
    public long? VendedorId { get; private set; }
    public decimal PctComisionVendedor { get; private set; }

    // ─── Control ──────────────────────────────────────────────────────────────
    public bool Activo { get; private set; } = true;

    // ─── Constructor privado (EF Core) ────────────────────────────────────────
    private Tercero() { }

    // ─── Factory ──────────────────────────────────────────────────────────────
    public static Tercero Crear(
        string legajo,
        string razonSocial,
        long tipoDocumentoId,
        string nroDocumento,
        long condicionIvaId,
        bool esCliente,
        bool esProveedor,
        bool esEmpleado,
        long? sucursalId,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(legajo, nameof(legajo));
        ArgumentException.ThrowIfNullOrWhiteSpace(razonSocial, nameof(razonSocial));
        ArgumentException.ThrowIfNullOrWhiteSpace(nroDocumento, nameof(nroDocumento));

        if (!esCliente && !esProveedor && !esEmpleado)
            throw new ArgumentException(
                "El tercero debe tener al menos un rol activo.");

        var t = new Tercero
        {
            Legajo          = legajo.Trim().ToUpperInvariant(),
            RazonSocial     = razonSocial.Trim(),
            TipoDocumentoId = tipoDocumentoId,
            NroDocumento    = nroDocumento.Trim(),
            CondicionIvaId  = condicionIvaId,
            EsCliente       = esCliente,
            EsProveedor     = esProveedor,
            EsEmpleado      = esEmpleado,
            SucursalId      = sucursalId,
            Activo          = true,
            Facturable      = true
        };

        t.SetCreated(userId);
        t.AddDomainEvent(new TerceroCreadoEvent(t.Id, t.Legajo, t.RazonSocial));

        return t;
    }

    // ─── Comando Actualizar ───────────────────────────────────────────────────
    public void Actualizar(
        string razonSocial,
        string? nombreFantasia,
        long condicionIvaId,
        string? telefono,
        string? celular,
        string? email,
        string? web,
        Domicilio domicilio,
        string? nroIngresosBrutos,
        string? nroMunicipal,
        decimal? limiteCredito,
        bool facturable,
        long? cobradorId,
        decimal pctComisionCobrador,
        long? vendedorId,
        decimal pctComisionVendedor,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(razonSocial, nameof(razonSocial));
        ValidarPorcentaje(pctComisionCobrador, nameof(pctComisionCobrador));
        ValidarPorcentaje(pctComisionVendedor, nameof(pctComisionVendedor));

        RazonSocial           = razonSocial.Trim();
        NombreFantasia        = nombreFantasia?.Trim();
        CondicionIvaId        = condicionIvaId;
        Telefono              = telefono?.Trim();
        Celular               = celular?.Trim();
        Email                 = email?.Trim().ToLowerInvariant();
        Web                   = web?.Trim();
        Domicilio             = domicilio;
        NroIngresosBrutos     = nroIngresosBrutos?.Trim();
        NroMunicipal          = nroMunicipal?.Trim();
        LimiteCredito         = limiteCredito;
        Facturable            = facturable;
        CobradorId            = cobradorId;
        PctComisionCobrador   = pctComisionCobrador;
        VendedorId            = vendedorId;
        PctComisionVendedor   = pctComisionVendedor;
        Observacion           = observacion?.Trim();

        SetUpdated(userId);
    }

    // ─── Actualización simple (setter alternativo, útil para migraciones/tests) ──
    public void Actualizar(
        string razonSocial,
        string? nombreFantasia,
        string? telefono,
        string? celular,
        string? email,
        string? web,
        Domicilio domicilio,
        string? nroIngresosBrutos,
        string? nroMunicipal,
        decimal? limiteCredito,
        bool facturable,
        string? observacion,
        long? userId)
    {
        RazonSocial       = razonSocial.Trim();
        NombreFantasia    = nombreFantasia?.Trim();
        Telefono          = telefono?.Trim();
        Celular           = celular?.Trim();
        Email             = email?.Trim().ToLowerInvariant();
        Web               = web?.Trim();
        Domicilio         = domicilio;
        NroIngresosBrutos = nroIngresosBrutos?.Trim();
        NroMunicipal      = nroMunicipal?.Trim();
        LimiteCredito     = limiteCredito;
        Facturable        = facturable;
        Observacion       = observacion?.Trim();
        SetUpdated(userId);
    }

    // ─── Roles ────────────────────────────────────────────────────────────────
    public void ActualizarRoles(
        bool esCliente,
        bool esProveedor,
        bool esEmpleado,
        long? userId)
    {
        if (!esCliente && !esProveedor && !esEmpleado)
            throw new ArgumentException(
                "El tercero debe tener al menos un rol activo.");

        EsCliente   = esCliente;
        EsProveedor = esProveedor;
        EsEmpleado  = esEmpleado;
        SetUpdated(userId);
        AddDomainEvent(new TerceroRolesActualizadosEvent(
            Id, Legajo, EsCliente, EsProveedor, EsEmpleado));
    }

    // ─── Datos de identificación sensibles ───────────────────────────────────
    public void ActualizarNroDocumento(string nroDocumento, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroDocumento, nameof(nroDocumento));
        NroDocumento = nroDocumento.Trim();
        SetUpdated(userId);
    }

    // ─── Asignación de opcionales ─────────────────────────────────────────────
    public void SetMoneda(long? monedaId) => MonedaId    = monedaId;
    public void SetCategoria(long? categoriaId) => CategoriaId = categoriaId;
    public void SetSucursal(long? sucursalId) => SucursalId  = sucursalId;
    public void SetDomicilio(Domicilio domicilio) => Domicilio = domicilio;
    public void SetNroIngresosBrutos(string? nro) => NroIngresosBrutos = nro?.Trim();

    // ─── Cobrador/Vendedor con validación ─────────────────────────────────────
    public void SetCobrador(long? cobradorId, decimal pctComision)
    {
        ValidarPorcentaje(pctComision, nameof(pctComision));
        CobradorId          = cobradorId;
        PctComisionCobrador = pctComision;
    }

    public void SetCobrador(long? cobradorId, decimal pctComision, long? userId)
    {
        ValidarPorcentaje(pctComision, nameof(pctComision));
        CobradorId           = cobradorId;
        PctComisionCobrador  = pctComision;
        SetUpdated(userId);
    }

    public void SetVendedor(long? vendedorId, decimal pctComision)
    {
        ValidarPorcentaje(pctComision, nameof(pctComision));
        VendedorId          = vendedorId;
        PctComisionVendedor = pctComision;
    }

    public void SetVendedor(long? vendedorId, decimal pctComision, long? userId)
    {
        ValidarPorcentaje(pctComision, nameof(pctComision));
        VendedorId           = vendedorId;
        PctComisionVendedor  = pctComision;
        SetUpdated(userId);
    }

    // ─── Activar / Desactivar ─────────────────────────────────────────────────
    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
        AddDomainEvent(new TerceroDesactivadoEvent(Id, Legajo, RazonSocial));
    }

    public void Activar(long? userId)
    {
        Activo = true;
        ClearDeletedAt();
        SetUpdated(userId);
        AddDomainEvent(new TerceroReactivadoEvent(Id, Legajo, RazonSocial));
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────
    private static void ValidarPorcentaje(decimal valor, string paramName)
    {
        if (valor < 0 || valor > 100)
            throw new ArgumentOutOfRangeException(paramName,
                "El porcentaje debe estar entre 0 y 100.");
    }

    private void ClearDeletedAt() => base.SetDeletedAt(null);
}
