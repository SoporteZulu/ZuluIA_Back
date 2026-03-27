using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.BI;

/// <summary>
/// Campo / dimensión de un cubo de análisis.
/// Migrado desde VB6: BI_CUBOCAMPO (campos del cubo disenable).
/// </summary>
public class CuboCampo : BaseEntity
{
    public long   CuboId                  { get; private set; }  // cub_id
    public string SourceName              { get; private set; } = "";  // ccam_SourceName
    public int?   Ubicacion              { get; private set; }   // ccam_Location (orientacion)
    public int?   Posicion               { get; private set; }   // ccam_Position
    public int?   TipoOrden             { get; private set; }    // ccam_SortType
    public int?   RankOn                { get; private set; }    // ccam_RankOn
    public int?   RankStyle             { get; private set; }    // ccam_RankStyle
    public bool   PiePaginaVisible      { get; private set; }    // ccam_GroupFooterVisible
    public int?   PiePaginaTipo         { get; private set; }    // ccam_GroupFooterType
    public string? PiePaginaCaption     { get; private set; }    // ccam_GroupFooterCaption
    public int?   FuncionAgregado       { get; private set; }    // ccam_AggregateFunc
    public string? Descripcion          { get; private set; }    // ccam_descripcion
    public string? Observacion          { get; private set; }    // ccam_observacion
    public bool   Calculado             { get; private set; }    // ccam_Calculated
    public bool   Visible               { get; private set; } = true; // ccam_Visible
    public string? VarName              { get; private set; }    // ccam_VarName
    public string? Filtro               { get; private set; }    // ccam_Filtro
    public long?  CampoPadreId         { get; private set; }     // ccam_idPadre
    public int    Orden                 { get; private set; }    // ccam_Orden

    private CuboCampo() { }

    public static CuboCampo Crear(
        long cuboId,
        string sourceName,
        string? descripcion       = null,
        int? ubicacion            = null,
        int? posicion             = null,
        bool visible              = true,
        bool calculado            = false,
        string? filtro            = null,
        long? campoPadreId        = null,
        int orden                 = 0)
    {
        if (cuboId <= 0) throw new ArgumentException("CuboId es requerido.");
        if (string.IsNullOrWhiteSpace(sourceName)) throw new ArgumentException("SourceName es requerido.");
        return new CuboCampo
        {
            CuboId       = cuboId,
            SourceName   = sourceName.Trim(),
            Descripcion  = descripcion,
            Ubicacion    = ubicacion,
            Posicion     = posicion,
            Visible      = visible,
            Calculado    = calculado,
            Filtro       = filtro,
            CampoPadreId = campoPadreId,
            Orden        = orden
        };
    }

    public void Actualizar(
        string? descripcion,
        int? ubicacion,
        int? posicion,
        bool visible,
        bool calculado,
        string? filtro,
        long? campoPadreId,
        int orden,
        int? tipoOrden,
        int? funcionAgregado)
    {
        Descripcion      = descripcion;
        Ubicacion        = ubicacion;
        Posicion         = posicion;
        Visible          = visible;
        Calculado        = calculado;
        Filtro           = filtro;
        CampoPadreId     = campoPadreId;
        Orden            = orden;
        TipoOrden        = tipoOrden;
        FuncionAgregado  = funcionAgregado;
    }
}
