using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.UnitTests.Domain;

public class EstadoPersonaCatalogoTests
{
    [Fact]
    public void Crear_ConDescripcionValida_DebeInicializarActivo()
    {
        var estado = EstadoPersonaCatalogo.Crear("Habilitado", userId: null);

        estado.Descripcion.Should().Be("Habilitado");
        estado.Activo.Should().BeTrue();
    }

    [Fact]
    public void Desactivar_DebeMarcarInactivo()
    {
        var estado = EstadoPersonaCatalogo.Crear("Suspendido", userId: null);

        estado.Desactivar(userId: null);

        estado.Activo.Should().BeFalse();
        estado.IsDeleted.Should().BeTrue();
    }
}
