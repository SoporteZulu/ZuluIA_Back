namespace ZuluIA_Back.Application.Features.Finanzas.DTOs;

public class CobroDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public string SucursalDescripcion { get; set; } = string.Empty;
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public string? TerceroLegajo { get; set; }
    public string? TerceroCuit { get; set; }
    public string? TerceroCondicionIva { get; set; }
    public string? TerceroDomicilioSnapshot { get; set; }
    public DateOnly Fecha { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Cotizacion { get; set; }
    public decimal Total { get; set; }
    public string? Observacion { get; set; }
    public string? ObservacionInterna { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? NroCierre { get; set; }

    // Comerciales
    public long? VendedorId { get; set; }
    public string? VendedorNombre { get; set; }
    public string? VendedorLegajo { get; set; }
    public long? CobradorId { get; set; }
    public string? CobradorNombre { get; set; }
    public string? CobradorLegajo { get; set; }
    public long? ZonaComercialId { get; set; }
    public string? ZonaComercialDescripcion { get; set; }

    // Operativos
    public long? UsuarioCajeroId { get; set; }
    public string? UsuarioCajeroNombre { get; set; }
    public string? VentanillaTurno { get; set; }
    public string TipoCobro { get; set; } = string.Empty;

    // Totales por forma de pago
    public decimal TotalEfectivo { get; set; }
    public decimal TotalCheques { get; set; }
    public decimal TotalElectronico { get; set; }

    // Auditoría
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedByUsuario { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public string? UpdatedByUsuario { get; set; }

    public IReadOnlyList<CobroMedioDto> Medios { get; set; } = [];
}

public class CobroMedioDto
{
    public long Id { get; set; }
    public long CajaId { get; set; }
    public string CajaDescripcion { get; set; } = string.Empty;
    public long FormaPagoId { get; set; }
    public string FormaPagoDescripcion { get; set; } = string.Empty;
    public long? ChequeId { get; set; }
    public string? ChequeNumero { get; set; }
    public string? ChequeBanco { get; set; }
    public decimal Importe { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Cotizacion { get; set; }

    // Transferencias
    public string? BancoOrigen { get; set; }
    public string? BancoDestino { get; set; }
    public string? NumeroOperacion { get; set; }
    public DateOnly? FechaAcreditacion { get; set; }

    // Tarjetas
    public string? TerminalPOS { get; set; }
    public string? NumeroCupon { get; set; }
    public string? NumeroLote { get; set; }
    public string? CodigoAutorizacion { get; set; }
    public int? CantidadCuotas { get; set; }
    public string? PlanCuotas { get; set; }
    public DateOnly? FechaAcreditacionEstimada { get; set; }
}