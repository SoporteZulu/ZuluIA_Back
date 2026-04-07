using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Diagnosticos;

public class PlantillaDiagnosticaVariable : BaseEntity
{
    public long PlantillaId { get; private set; }
    public long VariableId { get; private set; }
    public short Orden { get; private set; }

    private PlantillaDiagnosticaVariable() { }

    public static PlantillaDiagnosticaVariable Crear(long plantillaId, long variableId, short orden)
    {
        return new PlantillaDiagnosticaVariable
        {
            PlantillaId = plantillaId,
            VariableId = variableId,
            Orden = orden
        };
    }
}
