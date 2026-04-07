namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

/// <summary>
/// DTO optimizado para selector de clientes en módulos de ventas.
/// Contiene solo campos esenciales para búsqueda y selección rápida.
/// Usado en: Pedidos, Remitos, Facturas, Presupuestos.
/// </summary>
public class ClienteSelectorVentasDto
{
    // ─── Identificación mínima ────────────────────────────────────────────────
    public long Id { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string NroDocumento { get; set; } = string.Empty;
    
    // ─── Validación operativa para ventas ────────────────────────────────────
    public bool PuedeVender { get; set; }
    public string? MotivoBloqueo { get; set; }
    public bool Facturable { get; set; }
    
    // ─── Datos comerciales esenciales ─────────────────────────────────────────
    public long? MonedaId { get; set; }
    public string? MonedaDescripcion { get; set; }
    public long CondicionIvaId { get; set; }
    public string CondicionIvaDescripcion { get; set; } = string.Empty;
    public decimal? LimiteCredito { get; set; }
    public decimal? PorcentajeMaximoDescuento { get; set; }
    public bool TienePerfilComercial { get; set; }
    public string RiesgoCrediticio { get; set; } = "NORMAL";
    public decimal? SaldoMaximoVigente { get; set; }
    public string? CondicionCobranza { get; set; }
    public string? CondicionVenta { get; set; }
    public string? PlazoCobro { get; set; }
    public string? FacturadorPorDefecto { get; set; }
    public string? ObservacionComercial { get; set; }
    
    // ─── Contacto rápido ──────────────────────────────────────────────────────
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    
    // ─── Ubicación resumida ───────────────────────────────────────────────────
    public string UbicacionCompleta { get; set; } = string.Empty;
    public bool TieneSucursalesEntrega { get; set; }
    public string? SucursalEntregaPrincipalDescripcion { get; set; }
    public bool RequiereDefinirEntrega { get; set; }
    
    // ─── Comercial ────────────────────────────────────────────────────────────
    public long? VendedorId { get; set; }
    public string? VendedorNombre { get; set; }
}
