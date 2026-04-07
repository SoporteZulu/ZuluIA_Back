using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Franquicias;

/// <summary>
/// Asignación de una franquicia (sucursal) a una región comercial.
/// Migrado desde VB6: FRA_FRANQUICIASXREGION.
/// </summary>
public class FranquiciaXRegion : BaseEntity
{
    public long SucursalId     { get; private set; }
    public long RegionId       { get; private set; }
    public long? GrupoEconomicoId { get; private set; }

    private FranquiciaXRegion() { }

    public static FranquiciaXRegion Crear(long sucursalId, long regionId, long? grupoEconomicoId)
    {
        return new FranquiciaXRegion
        {
            SucursalId       = sucursalId,
            RegionId         = regionId,
            GrupoEconomicoId = grupoEconomicoId
        };
    }

    public void Actualizar(long sucursalId, long regionId, long? grupoEconomicoId)
    {
        SucursalId = sucursalId;
        RegionId = regionId;
        GrupoEconomicoId = grupoEconomicoId;
    }

    public void AsignarGrupo(long grupoId) => GrupoEconomicoId = grupoId;
}
