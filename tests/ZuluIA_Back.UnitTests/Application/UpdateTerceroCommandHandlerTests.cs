using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class UpdateTerceroCommandHandlerTests
{
    [Fact]
    public async Task Handle_DebeActualizarElDomicilioPersistidoCuandoNoSeInformanDomicilios()
    {
        var fixture = CreateFixture();

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        fixture.DomicilioExistente.Calle.Should().Be("Nueva 123");
    }

    [Fact]
    public async Task Handle_DebeConservarElTipoDomicilioExistenteCuandoSincronizaElPrincipal()
    {
        var fixture = CreateFixture();

        await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        fixture.DomicilioExistente.TipoDomicilioId.Should().Be(9);
    }

    private static TestFixture CreateFixture()
    {
        var repo = Substitute.For<ITerceroRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var db = Substitute.For<IApplicationDbContext>();

        currentUser.UserId.Returns(7);

        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30111111111", 1, true, false, false, null, currentUser.UserId);
        SetId(tercero, 12);

        var domicilioExistente = PersonaDomicilio.Crear(tercero.Id, 9, 3, 5, "Vieja 456", null, "5000", null, 0, true);
        SetId(domicilioExistente, 40);

        var provincia = Provincia.Crear(1, "CBA", "Cordoba");
        SetId(provincia, 3);

        var localidad = Localidad.Crear(provincia.Id, "Cordoba");
        SetId(localidad, 5);

        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet([provincia]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet([localidad]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>());
        db.PersonasDomicilios.Returns(MockDbSetHelper.CreateMockDbSet([domicilioExistente]));
        db.CategoriasClientes.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaCliente>());
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>());
        db.CategoriasProveedores.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaProveedor>());
        db.EstadosProveedores.Returns(MockDbSetHelper.CreateMockDbSet<EstadoProveedor>());
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>());
        db.EstadosCiviles.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCivilCatalogo>());
        db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet<TerceroPerfilComercial>());
        db.TercerosContactos.Returns(MockDbSetHelper.CreateMockDbSet<TerceroContacto>());
        db.TercerosSucursalesEntrega.Returns(MockDbSetHelper.CreateMockDbSet<TerceroSucursalEntrega>());
        db.TercerosTransportes.Returns(MockDbSetHelper.CreateMockDbSet<TerceroTransporte>());
        db.TercerosVentanasCobranza.Returns(MockDbSetHelper.CreateMockDbSet<TerceroVentanaCobranza>());
        db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet([CreateCondicionIva(1, 5, "Consumidor final")]));
        db.TiposDocumento.Returns(MockDbSetHelper.CreateMockDbSet([CreateTipoDocumento(1, 99, "Consumidor final")]));

        repo.GetByIdAsync(tercero.Id, Arg.Any<CancellationToken>()).Returns(tercero);

        uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Func<CancellationToken, Task>>(0).Invoke(callInfo.ArgAt<CancellationToken>(1)));
        uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new UpdateTerceroCommandHandler(repo, uow, currentUser, db);

        var command = new UpdateTerceroCommand(
            Id: tercero.Id,
            RazonSocial: "Empresa SA",
            NombreFantasia: null,
            TipoPersoneria: null,
            Nombre: null,
            Apellido: null,
            Tratamiento: null,
            Profesion: null,
            EstadoPersonaId: null,
            EstadoCivilId: null,
            EstadoCivil: null,
            Nacionalidad: null,
            Sexo: null,
            FechaNacimiento: null,
            FechaRegistro: null,
            EsEntidadGubernamental: false,
            ClaveFiscal: null,
            ValorClaveFiscal: null,
            NroDocumento: null,
            CondicionIvaId: 1,
            EsCliente: true,
            EsProveedor: false,
            EsEmpleado: false,
            Calle: "Nueva 123",
            Nro: null,
            Piso: null,
            Dpto: null,
            CodigoPostal: "5001",
            PaisId: null,
            ProvinciaId: null,
            LocalidadId: 5,
            BarrioId: null,
            NroIngresosBrutos: null,
            NroMunicipal: null,
            Telefono: null,
            Celular: null,
            Email: null,
            Web: null,
            MonedaId: null,
            CategoriaId: null,
            CategoriaClienteId: null,
            EstadoClienteId: null,
            CategoriaProveedorId: null,
            EstadoProveedorId: null,
            LimiteCredito: null,
            PorcentajeMaximoDescuento: null,
            VigenciaCreditoDesde: null,
            VigenciaCreditoHasta: null,
            Facturable: true,
            CobradorId: null,
            AplicaComisionCobrador: false,
            PctComisionCobrador: 0m,
            VendedorId: null,
            AplicaComisionVendedor: false,
            PctComisionVendedor: 0m,
            Observacion: null);

        return new TestFixture(handler, command, domicilioExistente);
    }

    private static CondicionIva CreateCondicionIva(long id, short codigo, string descripcion)
    {
        var entity = (CondicionIva)Activator.CreateInstance(typeof(CondicionIva), nonPublic: true)!;
        SetId(entity, id);
        typeof(CondicionIva).GetProperty(nameof(CondicionIva.Codigo))!.SetValue(entity, codigo);
        typeof(CondicionIva).GetProperty(nameof(CondicionIva.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }

    private static TipoDocumento CreateTipoDocumento(long id, short codigo, string descripcion)
    {
        var entity = (TipoDocumento)Activator.CreateInstance(typeof(TipoDocumento), nonPublic: true)!;
        SetId(entity, id);
        typeof(TipoDocumento).GetProperty(nameof(TipoDocumento.Codigo))!.SetValue(entity, codigo);
        typeof(TipoDocumento).GetProperty(nameof(TipoDocumento.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }

    private static void SetId(object entity, long id)
        => entity.GetType().BaseType!.GetProperty("Id")!.SetValue(entity, id);

    private sealed record TestFixture(
        UpdateTerceroCommandHandler Handler,
        UpdateTerceroCommand Command,
        PersonaDomicilio DomicilioExistente);
}
