namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

/// <summary>
/// DTO mínimo para combos, selectores y autocomplete.
/// Equivalente al contenido de los ComboBox de clientes/proveedores del VB6
/// (solo mostraba Legajo + Razón Social en el listado desplegable).
/// Usado en endpoints de otros módulos que necesitan seleccionar un tercero:
///   GET /api/terceros/clientes-activos
///   GET /api/terceros/proveedores-activos
/// </summary>
public class TerceroSelectorDto
{
    public long Id { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string NroDocumento { get; set; } = string.Empty;
    public string GeografiaCompleta { get; set; } = string.Empty;
    public string UbicacionCompleta { get; set; } = string.Empty;

    /// <summary>
    /// Texto para mostrar en el combo: "00001 — Empresa SA (20-12345678-9)"
    /// </summary>
    public string Display => $"{Legajo} — {RazonSocial} ({NroDocumento})";
}