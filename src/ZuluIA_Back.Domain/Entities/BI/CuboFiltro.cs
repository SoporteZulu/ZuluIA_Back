using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.BI;

/// <summary>
/// Filtro aplicado a un cubo de análisis.
/// Migrado desde VB6: BI_CUBOFILTROS.
/// </summary>
public class CuboFiltro : BaseEntity
{
    public long   CuboId    { get; private set; }  // cub_id
    public string Filtro    { get; private set; } = "";  // cfil_filtro
    public int    Operador  { get; private set; } = 1;   // cfil_operador (1=AND, 2=OR)
    public int    Orden     { get; private set; }         // cfil_orden

    private CuboFiltro() { }

    public static CuboFiltro Crear(long cuboId, string filtro, int operador = 1, int orden = 0)
    {
        if (cuboId <= 0) throw new ArgumentException("CuboId es requerido.");
        if (string.IsNullOrWhiteSpace(filtro)) throw new ArgumentException("El filtro es requerido.");
        return new CuboFiltro
        {
            CuboId   = cuboId,
            Filtro   = filtro.Trim(),
            Operador = operador,
            Orden    = orden
        };
    }

    public void Actualizar(string filtro, int operador, int orden)
    {
        if (string.IsNullOrWhiteSpace(filtro)) throw new ArgumentException("El filtro es requerido.");
        Filtro   = filtro.Trim();
        Operador = operador;
        Orden    = orden;
    }
}
