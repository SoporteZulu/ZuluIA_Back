using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Geografia;

/// <summary>
/// Región geográfica/comercial con estructura jerárquica.
/// Migrado desde VB6: clsRegiones / FRA_REGIONES.
/// </summary>
public class Region : BaseEntity
{
    public string  Codigo               { get; private set; } = string.Empty;
    public string  Descripcion          { get; private set; } = string.Empty;
    public long?   RegionIntegradoraId  { get; private set; }
    public int     Orden                { get; private set; }
    public int     Nivel                { get; private set; }
    public string? CodigoEstructura     { get; private set; }
    public bool    EsRegionIntegradora  { get; private set; }
    public string? Observacion          { get; private set; }

    private Region() { }

    public static Region Crear(string codigo, string descripcion,
        long? regionIntegradoraId = null, int orden = 0, int nivel = 0,
        string? codigoEstructura = null, bool esRegionIntegradora = false,
        string? observacion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Region
        {
            Codigo              = codigo.Trim().ToUpperInvariant(),
            Descripcion         = descripcion.Trim(),
            RegionIntegradoraId = regionIntegradoraId,
            Orden               = orden,
            Nivel               = nivel,
            CodigoEstructura    = codigoEstructura?.Trim(),
            EsRegionIntegradora = esRegionIntegradora,
            Observacion         = observacion?.Trim()
        };
    }

    public void Actualizar(string descripcion, long? regionIntegradoraId,
        int orden, int nivel, string? codigoEstructura,
        bool esRegionIntegradora, string? observacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion         = descripcion.Trim();
        RegionIntegradoraId = regionIntegradoraId;
        Orden               = orden;
        Nivel               = nivel;
        CodigoEstructura    = codigoEstructura?.Trim();
        EsRegionIntegradora = esRegionIntegradora;
        Observacion         = observacion?.Trim();
    }
}
