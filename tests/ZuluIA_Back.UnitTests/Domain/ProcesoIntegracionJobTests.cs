using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ProcesoIntegracionJobTests
{
    [Fact]
    public void Finalizar_ConErrores_DebeQuedarEnFinalizadoConErrores()
    {
        var job = ProcesoIntegracionJob.Crear(TipoProcesoIntegracion.ImportacionClientes, "Import", 2, null, null);
        job.Iniciar(null);
        job.RegistrarExito(null);
        job.RegistrarError("error", null);
        job.Finalizar(null, null);
        job.Estado.Should().Be(EstadoProcesoIntegracion.FinalizadoConErrores);
    }

    [Fact]
    public void Finalizar_SinErrores_DebeQuedarFinalizado()
    {
        var job = ProcesoIntegracionJob.Crear(TipoProcesoIntegracion.ImportacionClientes, "Import", 1, null, null);
        job.Iniciar(null);
        job.RegistrarExito(null);

        job.Finalizar(null, null);

        job.Estado.Should().Be(EstadoProcesoIntegracion.Finalizado);
    }
}
