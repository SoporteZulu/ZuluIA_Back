using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Common.Mappings;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;
using ZuluIA_Back.UnitTests.Helpers;
using CondicionIvaEntity = ZuluIA_Back.Domain.Entities.Referencia.CondicionIva;

namespace ZuluIA_Back.UnitTests.Application;

public class GetClientesSelectorVentasQueryHandlerTests
{
    [Fact]
    public async Task Handle_CuandoClienteActivo_DebePuedeVenderTrue()
    {
        var (handler, db, _) = CreateHandler();
        
        var cliente = Tercero.Crear("CLI001", "Cliente Activo SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 1L);

        var terceros = MockDbSetHelper.CreateMockDbSet([cliente]);
        db.Terceros.Returns(terceros);
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIvaEntity>(
            [CreateCondicionIva(1, "Responsable Inscripto")]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>([]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>([]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>([]));

        var result = await handler.Handle(
            new GetClientesSelectorVentasQuery(),
            CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
        result[0].Legajo.Should().Be("CLI001");
        result[0].PuedeVender.Should().BeTrue();
        result[0].MotivoBloqueo.Should().BeNull();
        result[0].Facturable.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CuandoClienteInactivo_DebePuedeVenderFalse()
    {
        var (handler, db, _) = CreateHandler();
        
        var cliente = Tercero.Crear("CLI002", "Cliente Inactivo SA", 1, "30-22222222-2", 1, true, false, false, null, null);
        cliente.Desactivar(null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 2L);

        var terceros = MockDbSetHelper.CreateMockDbSet([cliente]);
        db.Terceros.Returns(terceros);
        
        // No debería aparecer en el selector porque soloActivos=true por defecto
        var result = await handler.Handle(
            new GetClientesSelectorVentasQuery(),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CuandoClienteBloqueado_DebePuedeVenderFalseConMotivo()
    {
        var (handler, db, _) = CreateHandler();
        
        var cliente = Tercero.Crear("CLI003", "Cliente Bloqueado SA", 1, "30-33333333-3", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 3L);
        typeof(Tercero).GetProperty(nameof(Tercero.EstadoClienteId))!.SetValue(cliente, 5L);

        var terceros = MockDbSetHelper.CreateMockDbSet([cliente]);
        db.Terceros.Returns(terceros);
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIvaEntity>(
            [CreateCondicionIva(1, "Responsable Inscripto")]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet(
            [CreateEstadoCliente(5, "Moroso", true, true)]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>([]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>([]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>([]));

        var result = await handler.Handle(
            new GetClientesSelectorVentasQuery(),
            CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].PuedeVender.Should().BeFalse();
        result[0].MotivoBloqueo.Should().Contain("Moroso");
    }

    [Fact]
    public async Task Handle_CuandoClienteNoFacturable_DebePuedeVenderFalseConMotivo()
    {
        var (handler, db, _) = CreateHandler();
        
        var cliente = Tercero.Crear("CLI004", "Cliente No Facturable SA", 1, "30-44444444-4", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 4L);
        typeof(Tercero).GetProperty(nameof(Tercero.Facturable))!.SetValue(cliente, false);

        var terceros = MockDbSetHelper.CreateMockDbSet([cliente]);
        db.Terceros.Returns(terceros);
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIvaEntity>(
            [CreateCondicionIva(1, "Responsable Inscripto")]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>([]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>([]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>([]));

        var result = await handler.Handle(
            new GetClientesSelectorVentasQuery(),
            CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].PuedeVender.Should().BeFalse();
        result[0].MotivoBloqueo.Should().Be("Cliente no facturable");
    }

    [Fact]
    public async Task Handle_ConUbicacionCompleta_DebeFormatearCorrectamente()
    {
        var (handler, db, _) = CreateHandler();
        
        var cliente = Tercero.Crear("CLI001", "Cliente con Ubicación", 1, "30-11111111-1", 1, true, false, false, null, null);
        cliente.SetDomicilio(Domicilio.Crear("Av. Siempre Viva", "742", null, null, "5000", 5, 7, 2));
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 1L);

        var provincia = CreateProvincia(2, "Cordoba");
        var localidad = CreateLocalidad(5, 2, "Capital");
        var barrio = CreateBarrio(7, 5, "Centro");

        var terceros = MockDbSetHelper.CreateMockDbSet([cliente]);
        db.Terceros.Returns(terceros);
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIvaEntity>(
            [CreateCondicionIva(1, "Responsable Inscripto")]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet([localidad]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet([provincia]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet([barrio]));

        var result = await handler.Handle(
            new GetClientesSelectorVentasQuery(),
            CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].UbicacionCompleta.Should().Contain("Av. Siempre Viva 742");
        result[0].UbicacionCompleta.Should().Contain("Centro");
        result[0].UbicacionCompleta.Should().Contain("Capital");
        result[0].UbicacionCompleta.Should().Contain("Cordoba");
        result[0].UbicacionCompleta.Should().Contain("CP 5000");
    }

    [Fact]
    public async Task Handle_CuandoTieneSucursalEntrega_DebeExponerDatosEntrega()
    {
        var (handler, db, _) = CreateHandler();

        var cliente = Tercero.Crear("CLI005", "Cliente con Entrega", 1, "30-55555555-5", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 5L);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([cliente]));
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIvaEntity>(
            [CreateCondicionIva(1, "Responsable Inscripto")]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>([]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>([]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>([]));
        db.TercerosSucursalesEntrega.Returns(MockDbSetHelper.CreateMockDbSet(
            [CreateSucursalEntrega(1, 5, "Casa Central", true, 0)]));

        var result = await handler.Handle(new GetClientesSelectorVentasQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].TieneSucursalesEntrega.Should().BeTrue();
        result[0].SucursalEntregaPrincipalDescripcion.Should().Be("Casa Central");
        result[0].RequiereDefinirEntrega.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CuandoNoTieneSucursalEntrega_DebeInformarloEnElSelector()
    {
        var (handler, db, _) = CreateHandler();

        var cliente = Tercero.Crear("CLI006", "Cliente sin Entrega", 1, "30-66666666-6", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 6L);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([cliente]));
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIvaEntity>(
            [CreateCondicionIva(1, "Responsable Inscripto")]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>([]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>([]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>([]));
        db.TercerosSucursalesEntrega.Returns(MockDbSetHelper.CreateMockDbSet<TerceroSucursalEntrega>([]));

        var result = await handler.Handle(new GetClientesSelectorVentasQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].TieneSucursalesEntrega.Should().BeFalse();
        result[0].SucursalEntregaPrincipalDescripcion.Should().BeNull();
        result[0].RequiereDefinirEntrega.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CuandoExistePerfilComercial_DebeExponerDatosComercialesMinimos()
    {
        var (handler, db, _) = CreateHandler();

        var cliente = Tercero.Crear("CLI007", "Cliente Comercial SA", 1, "30-77777777-7", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 7L);

        var perfil = TerceroPerfilComercial.Crear(7L, null);
        perfil.Actualizar(
            null,
            null,
            null,
            null,
            "CTA CTE",
            ZuluIA_Back.Domain.Enums.RiesgoCrediticioComercial.Alerta,
            150000m,
            null,
            null,
            null,
            "Cuenta corriente",
            "30 días",
            "Mostrador",
            null,
            "Entregar solo contra firma",
            null);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([cliente]));
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIva>(
            [CreateCondicionIva(1, "Responsable Inscripto")]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>([]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>([]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>([]));
        db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet([perfil]));

        var result = await handler.Handle(new GetClientesSelectorVentasQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].TienePerfilComercial.Should().BeTrue();
        result[0].RiesgoCrediticio.Should().Be("ALERTA");
        result[0].SaldoMaximoVigente.Should().Be(150000m);
        result[0].CondicionCobranza.Should().Be("CTA CTE");
        result[0].CondicionVenta.Should().Be("Cuenta corriente");
        result[0].PlazoCobro.Should().Be("30 días");
        result[0].FacturadorPorDefecto.Should().Be("Mostrador");
        result[0].ObservacionComercial.Should().Be("Entregar solo contra firma");
    }

    [Fact]
    public async Task Handle_CuandoBuscaDocumentoSinSeparadores_DebeEncontrarCliente()
    {
        var (handler, db, _) = CreateHandler();

        var cliente = Tercero.Crear("CLI008", "Cliente Documento", 1, "30-88888888-8", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(cliente, 8L);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([cliente]));
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIvaEntity>(
            [CreateCondicionIva(1, "Responsable Inscripto")]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>([]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>([]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>([]));
        db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet<TerceroPerfilComercial>([]));

        var result = await handler.Handle(new GetClientesSelectorVentasQuery("30888888888"), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Legajo.Should().Be("CLI008");
    }

    private static (GetClientesSelectorVentasQueryHandler handler, IApplicationDbContext db, ITerceroRepository repo) CreateHandler()
    {
        var repo = Substitute.For<ITerceroRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

        db.TercerosSucursalesEntrega.Returns(MockDbSetHelper.CreateMockDbSet<TerceroSucursalEntrega>([]));
        db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet<TerceroPerfilComercial>([]));
        
        var handler = new GetClientesSelectorVentasQueryHandler(repo, db, mapper);
        
        return (handler, db, repo);
    }

    private static CondicionIvaEntity CreateCondicionIva(long id, string descripcion)
    {
        var entity = (CondicionIvaEntity)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(CondicionIvaEntity));
        typeof(CondicionIvaEntity).GetProperty(nameof(CondicionIvaEntity.Id))!.SetValue(entity, id);
        typeof(CondicionIvaEntity).GetProperty(nameof(CondicionIvaEntity.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }

    private static EstadoCliente CreateEstadoCliente(long id, string descripcion, bool bloquea, bool activo)
    {
        var entity = EstadoCliente.Crear("EC", descripcion, bloquea, null);
        typeof(EstadoCliente).GetProperty(nameof(EstadoCliente.Id))!.SetValue(entity, id);
        typeof(EstadoCliente).GetProperty(nameof(EstadoCliente.Activo))!.SetValue(entity, activo);
        return entity;
    }

    private static Provincia CreateProvincia(long id, string descripcion)
    {
        var entity = Provincia.Crear(1, "CBA", descripcion);
        typeof(Provincia).GetProperty(nameof(Provincia.Id))!.SetValue(entity, id);
        return entity;
    }

    private static Localidad CreateLocalidad(long id, long provinciaId, string descripcion)
    {
        var entity = Localidad.Crear(provinciaId, descripcion);
        typeof(Localidad).GetProperty(nameof(Localidad.Id))!.SetValue(entity, id);
        return entity;
    }

    private static Barrio CreateBarrio(long id, long localidadId, string descripcion)
    {
        var entity = Barrio.Crear(localidadId, descripcion);
        typeof(Barrio).GetProperty(nameof(Barrio.Id))!.SetValue(entity, id);
        return entity;
    }

    private static TerceroSucursalEntrega CreateSucursalEntrega(long id, long terceroId, string descripcion, bool principal, int orden)
    {
        var entity = TerceroSucursalEntrega.Crear(terceroId, descripcion, null, null, null, null, null, principal, orden, null);
        typeof(TerceroSucursalEntrega).GetProperty(nameof(TerceroSucursalEntrega.Id))!.SetValue(entity, id);
        return entity;
    }
}
