namespace ZuluIA_Back.Application.Features.Recibos.DTOs;

public class ReciboListDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public string SucursalDescripcion { get; set; } = string.Empty;
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public string? TerceroLegajo { get; set; }
    public DateOnly Fecha { get; set; }
    public string Serie { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string NumeroCompleto { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public long? CobroId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ReciboDto : ReciboListDto
{
    // Datos fiscales del tercero (snapshot)
    public string? TerceroCuit { get; set; }
    public string? TerceroCondicionIva { get; set; }
    public string? TerceroDomicilio { get; set; }

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

    // Observaciones
    public string? Observacion { get; set; }
    public string? LeyendaFiscal { get; set; }

    // Metadatos de impresión
    public string? FormatoImpresion { get; set; }
    public int? CopiasImpresas { get; set; }
    public DateTimeOffset? FechaImpresion { get; set; }

    // Auditoría
    public long? CreatedBy { get; set; }
    public string? CreatedByUsuario { get; set; }

    public IReadOnlyList<ReciboItemDto> Items { get; set; } = [];
}

public class ReciboItemDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    
    // Referencia a comprobante imputado (si corresponde)
    public long? ComprobanteImputadoId { get; set; }
    public string? ComprobanteImputadoNumero { get; set; }
    public string? ComprobanteImputadoTipo { get; set; }
}
