using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Domain.Entities.Terceros;

public class Tercero : AuditableEntity
{
    // ─── Identificación ───────────────────────────────────────────────────────
    public string Legajo { get; private set; } = string.Empty;
    public string RazonSocial { get; private set; } = string.Empty;
    public string? NombreFantasia { get; private set; }
    public TipoPersoneriaTercero TipoPersoneria { get; private set; } = TipoPersoneriaTercero.Juridica;
    public string? Nombre { get; private set; }
    public string? Apellido { get; private set; }
    public string? Tratamiento { get; private set; }
    public string? Profesion { get; private set; }
    public long? EstadoPersonaId { get; private set; }
    public long? EstadoCivilId { get; private set; }
    public string? EstadoCivil { get; private set; }
    public string? Nacionalidad { get; private set; }
    public string? Sexo { get; private set; }
    public DateOnly? FechaNacimiento { get; private set; }
    public DateOnly? FechaRegistro { get; private set; }
    public bool EsEntidadGubernamental { get; private set; }
    public string? ClaveFiscal { get; private set; }
    public string? ValorClaveFiscal { get; private set; }

    // ─── Documento e IVA ──────────────────────────────────────────────────────
    public long TipoDocumentoId { get; private set; }
    public string NroDocumento { get; private set; } = string.Empty;
    public long CondicionIvaId { get; private set; }

    // ─── Clasificación ────────────────────────────────────────────────────────
    public long? CategoriaId { get; private set; }
    public long? CategoriaClienteId { get; private set; }
    public long? EstadoClienteId { get; private set; }
    public long? CategoriaProveedorId { get; private set; }
    public long? EstadoProveedorId { get; private set; }
    public bool EsCliente { get; private set; }
    public bool EsProveedor { get; private set; }
    public bool EsEmpleado { get; private set; }

    // ─── Domicilio (Value Object) ─────────────────────────────────────────────
    public Domicilio Domicilio { get; private set; } = Domicilio.Vacio();
    public long? PaisId { get; private set; }

    // ─── Datos fiscales ───────────────────────────────────────────────────────
    public string? NroIngresosBrutos { get; private set; }
    public string? NroMunicipal { get; private set; }

    // ─── Contacto ─────────────────────────────────────────────────────────────
    public string? Telefono { get; private set; }
    public string? Celular { get; private set; }
    public string? Email { get; private set; }
    public string? Web { get; private set; }
    public long? UsuarioId { get; private set; }

    // ─── Comercial ────────────────────────────────────────────────────────────
    public long? MonedaId { get; private set; }
    public decimal? LimiteCredito { get; private set; }
    public decimal? PorcentajeMaximoDescuento { get; private set; }
    public DateOnly? VigenciaCreditoDesde { get; private set; }
    public DateOnly? VigenciaCreditoHasta { get; private set; }
    public bool Facturable { get; private set; } = true;
    public long? SucursalId { get; private set; }
    public string? Observacion { get; private set; }

    // ─── Cobrador/Vendedor ────────────────────────────────────────────────────
    public long? CobradorId { get; private set; }
    public bool AplicaComisionCobrador { get; private set; }
    public decimal PctComisionCobrador { get; private set; }
    public long? VendedorId { get; private set; }
    public bool AplicaComisionVendedor { get; private set; }
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

        if (!esCliente && !esProveedor && !esEmpleado)
            throw new ArgumentException(
                "El tercero debe tener al menos un rol activo.");

        var t = new Tercero
        {
            Legajo          = legajo.Trim().ToUpperInvariant(),
            RazonSocial     = razonSocial.Trim(),
            TipoDocumentoId = tipoDocumentoId,
            NroDocumento    = nroDocumento?.Trim() ?? string.Empty,
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
        decimal? porcentajeMaximoDescuento,
        DateOnly? vigenciaCreditoDesde,
        DateOnly? vigenciaCreditoHasta,
        bool facturable,
        long? cobradorId,
        bool aplicaComisionCobrador,
        decimal pctComisionCobrador,
        long? vendedorId,
        bool aplicaComisionVendedor,
        decimal pctComisionVendedor,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(razonSocial, nameof(razonSocial));
        ValidarPorcentaje(pctComisionCobrador, nameof(pctComisionCobrador));
        ValidarPorcentaje(pctComisionVendedor, nameof(pctComisionVendedor));
        ValidarPorcentajeOpcional(porcentajeMaximoDescuento, nameof(porcentajeMaximoDescuento));
        ValidarRangoVigencia(vigenciaCreditoDesde, vigenciaCreditoHasta, nameof(vigenciaCreditoDesde), nameof(vigenciaCreditoHasta));

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
        PorcentajeMaximoDescuento = porcentajeMaximoDescuento;
        VigenciaCreditoDesde  = vigenciaCreditoDesde;
        VigenciaCreditoHasta  = vigenciaCreditoHasta;
        Facturable            = facturable;
        CobradorId            = cobradorId;
        AplicaComisionCobrador = aplicaComisionCobrador;
        PctComisionCobrador   = aplicaComisionCobrador ? pctComisionCobrador : 0m;
        VendedorId            = vendedorId;
        AplicaComisionVendedor = aplicaComisionVendedor;
        PctComisionVendedor   = aplicaComisionVendedor ? pctComisionVendedor : 0m;
        Observacion           = observacion?.Trim();

        SetUpdated(userId);
    }

    public void ActualizarPersoneriaFiscal(
        TipoPersoneriaTercero tipoPersoneria,
        string? nombre,
        string? apellido,
        string? tratamiento,
        string? profesion,
        long? estadoCivilId,
        string? estadoCivil,
        string? nacionalidad,
        string? sexo,
        DateOnly? fechaNacimiento,
        bool esEntidadGubernamental,
        string? claveFiscal,
        string? valorClaveFiscal,
        long? userId)
    {
        if (tipoPersoneria == TipoPersoneriaTercero.Fisica)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nombre, nameof(nombre));
            ArgumentException.ThrowIfNullOrWhiteSpace(apellido, nameof(apellido));

            if (esEntidadGubernamental)
                throw new ArgumentException("Una persona física no puede marcarse como entidad gubernamental.", nameof(esEntidadGubernamental));

            ValidarSexo(sexo);
            ValidarFechaNacimiento(fechaNacimiento);
        }

        if (string.IsNullOrWhiteSpace(claveFiscal) ^ string.IsNullOrWhiteSpace(valorClaveFiscal))
            throw new ArgumentException("Debe informar tanto la clave fiscal como su valor.");

        TipoPersoneria = tipoPersoneria;
        Nombre = Normalize(nombre);
        Apellido = Normalize(apellido);
        Tratamiento = tipoPersoneria == TipoPersoneriaTercero.Fisica ? Normalize(tratamiento) : null;
        Profesion = tipoPersoneria == TipoPersoneriaTercero.Fisica ? Normalize(profesion) : null;
        EstadoCivilId = tipoPersoneria == TipoPersoneriaTercero.Fisica ? estadoCivilId : null;
        EstadoCivil = tipoPersoneria == TipoPersoneriaTercero.Fisica ? Normalize(estadoCivil) : null;
        Nacionalidad = tipoPersoneria == TipoPersoneriaTercero.Fisica ? Normalize(nacionalidad) : null;
        Sexo = tipoPersoneria == TipoPersoneriaTercero.Fisica ? NormalizeSexo(sexo) : null;
        FechaNacimiento = tipoPersoneria == TipoPersoneriaTercero.Fisica ? fechaNacimiento : null;
        EsEntidadGubernamental = esEntidadGubernamental;
        ClaveFiscal = Normalize(claveFiscal);
        ValorClaveFiscal = Normalize(valorClaveFiscal);
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
        decimal? porcentajeMaximoDescuento,
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
        PorcentajeMaximoDescuento = porcentajeMaximoDescuento;
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
    public void SetEstadoPersona(long? estadoPersonaId) => EstadoPersonaId = estadoPersonaId;
    public void SetEstadoCivil(long? estadoCivilId, string? estadoCivil)
    {
        EstadoCivilId = estadoCivilId;
        EstadoCivil = Normalize(estadoCivil);
    }
    public void SetCategoriaCliente(long? categoriaClienteId) => CategoriaClienteId = categoriaClienteId;
    public void SetEstadoCliente(long? estadoClienteId) => EstadoClienteId = estadoClienteId;
    public void SetCategoriaProveedor(long? categoriaProveedorId) => CategoriaProveedorId = categoriaProveedorId;
    public void SetEstadoProveedor(long? estadoProveedorId) => EstadoProveedorId = estadoProveedorId;
    public void SetSucursal(long? sucursalId) => SucursalId  = sucursalId;
    public void SetDomicilio(Domicilio domicilio) => Domicilio = domicilio;
    public void SetPais(long? paisId) => PaisId = paisId;
    public void SetFechaRegistro(DateOnly? fechaRegistro, long? userId)
    {
        ValidarFechaRegistro(fechaRegistro);
        FechaRegistro = fechaRegistro;
        SetUpdated(userId);
    }
    public void SetUsuario(long? usuarioId, long? userId)
    {
        UsuarioId = usuarioId;
        SetUpdated(userId);
    }
    public void SetNroIngresosBrutos(string? nro) => NroIngresosBrutos = nro?.Trim();
    public void SetVigenciaCredito(DateOnly? desde, DateOnly? hasta)
    {
        ValidarRangoVigencia(desde, hasta, nameof(desde), nameof(hasta));
        VigenciaCreditoDesde = desde;
        VigenciaCreditoHasta = hasta;
    }

    public void ActualizarCuentaCorriente(
        decimal? limiteCreditoTotal,
        DateOnly? vigenciaLimiteCreditoTotalDesde,
        DateOnly? vigenciaLimiteCreditoTotalHasta,
        long? userId)
    {
        if (limiteCreditoTotal.HasValue && limiteCreditoTotal.Value < 0)
            throw new ArgumentException("El límite de crédito total no puede ser negativo.", nameof(limiteCreditoTotal));

        ValidarRangoVigencia(
            vigenciaLimiteCreditoTotalDesde,
            vigenciaLimiteCreditoTotalHasta,
            nameof(vigenciaLimiteCreditoTotalDesde),
            nameof(vigenciaLimiteCreditoTotalHasta));

        LimiteCredito = limiteCreditoTotal;
        VigenciaCreditoDesde = vigenciaLimiteCreditoTotalDesde;
        VigenciaCreditoHasta = vigenciaLimiteCreditoTotalHasta;
        SetUpdated(userId);
    }

    // ─── Cobrador/Vendedor con validación ─────────────────────────────────────
    public void SetCobrador(long? cobradorId, decimal pctComision)
    {
        SetCobrador(cobradorId, cobradorId.HasValue || pctComision > 0, pctComision);
    }

    public void SetCobrador(long? cobradorId, decimal pctComision, long? userId)
    {
        SetCobrador(cobradorId, cobradorId.HasValue || pctComision > 0, pctComision, userId);
    }

    public void SetCobrador(long? cobradorId, bool aplicaComision, decimal pctComision)
    {
        ValidarPorcentaje(pctComision, nameof(pctComision));
        CobradorId = cobradorId;
        AplicaComisionCobrador = aplicaComision;
        PctComisionCobrador = aplicaComision ? pctComision : 0m;
    }

    public void SetCobrador(long? cobradorId, bool aplicaComision, decimal pctComision, long? userId)
    {
        SetCobrador(cobradorId, aplicaComision, pctComision);
        SetUpdated(userId);
    }

    public void SetVendedor(long? vendedorId, decimal pctComision)
    {
        SetVendedor(vendedorId, vendedorId.HasValue || pctComision > 0, pctComision);
    }

    public void SetVendedor(long? vendedorId, decimal pctComision, long? userId)
    {
        SetVendedor(vendedorId, vendedorId.HasValue || pctComision > 0, pctComision, userId);
    }

    public void SetVendedor(long? vendedorId, bool aplicaComision, decimal pctComision)
    {
        ValidarPorcentaje(pctComision, nameof(pctComision));
        VendedorId = vendedorId;
        AplicaComisionVendedor = aplicaComision;
        PctComisionVendedor = aplicaComision ? pctComision : 0m;
    }

    public void SetVendedor(long? vendedorId, bool aplicaComision, decimal pctComision, long? userId)
    {
        SetVendedor(vendedorId, aplicaComision, pctComision);
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

    private static void ValidarPorcentajeOpcional(decimal? valor, string paramName)
    {
        if (valor.HasValue)
            ValidarPorcentaje(valor.Value, paramName);
    }

    private static void ValidarSexo(string? sexo)
    {
        var normalizado = NormalizeSexo(sexo);
        if (normalizado is not null && normalizado is not ("M" or "F"))
            throw new ArgumentException("El sexo debe ser M o F.", nameof(sexo));
    }

    private static void ValidarFechaNacimiento(DateOnly? fechaNacimiento)
    {
        if (fechaNacimiento.HasValue && fechaNacimiento.Value > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("La fecha de nacimiento no puede ser futura.", nameof(fechaNacimiento));
    }

    private static void ValidarFechaRegistro(DateOnly? fechaRegistro)
    {
        if (fechaRegistro.HasValue && fechaRegistro.Value > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("La fecha de registro no puede ser futura.", nameof(fechaRegistro));
    }

    private static void ValidarRangoVigencia(DateOnly? desde, DateOnly? hasta, string desdeParamName, string hastaParamName)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            throw new ArgumentException("La vigencia desde no puede ser mayor que la vigencia hasta.", $"{desdeParamName},{hastaParamName}");
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeSexo(string? sexo)
        => string.IsNullOrWhiteSpace(sexo) ? null : sexo.Trim().ToUpperInvariant();

    private void ClearDeletedAt() => base.SetDeletedAt(null);
}
