using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Mappings;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;
using ZuluIA_Back.Infrastructure.Persistence;

namespace ZuluIA_Back.UnitTests.Application;

public class GetTerceroByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_CuandoExisteTercero_RetornaCuentaContableMediosContactoYResumenUsuarioCliente()
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        await using var db = CreateDbContext();

        var tipoDocumento = CreateTipoDocumento(1, 80, "CUIT");
        var condicionIva = CreateCondicionIva(1, 1, "Responsable Inscripto");
        db.TiposDocumento.Add(tipoDocumento);
        db.CondicionesIva.Add(condicionIva);

        var tercero = Tercero.Crear("CLI001", "Cliente Uno", tipoDocumento.Id, "30712345678", condicionIva.Id, true, false, false, null, null);
        tercero.Actualizar(
            razonSocial: "Cliente Uno",
            nombreFantasia: "Cliente Uno SA",
            condicionIvaId: condicionIva.Id,
            telefono: "021111111",
            celular: null,
            email: "cliente@demo.com",
            web: null,
            domicilio: Domicilio.Crear("Siempre Viva", "123", null, null, "5000", null, null, null),
            nroIngresosBrutos: "IB-123",
            nroMunicipal: "MUN-456",
            limiteCredito: 150000m,
            porcentajeMaximoDescuento: null,
            vigenciaCreditoDesde: null,
            vigenciaCreditoHasta: null,
            facturable: true,
            cobradorId: null,
            aplicaComisionCobrador: false,
            pctComisionCobrador: 0m,
            vendedorId: null,
            aplicaComisionVendedor: false,
            pctComisionVendedor: 0m,
            observacion: "Observación",
            userId: null);

        db.Terceros.Add(tercero);
        await db.SaveChangesAsync();

        var usuarioGrupo = Usuario.Crear("grupo-clientes", "Grupo Clientes", "grupo@demo.com", null, null);
        var usuarioCliente = Usuario.Crear("cliente-web", "Cliente Web", "cliente-web@demo.com", null, null);
        var cobrador = Usuario.Crear("cobra-uno", "Cobrador Uno", "cobrador@demo.com", null, null);
        var vendedor = Usuario.Crear("vende-uno", "Vendedor Uno", "vendedor@demo.com", null, null);
        usuarioCliente.EstablecerPasswordHash("hash-demo", null);

        db.Usuarios.AddRange(usuarioGrupo, usuarioCliente, cobrador, vendedor);
        await db.SaveChangesAsync();

        db.UsuariosXUsuario.Add(UsuarioXUsuario.Crear(usuarioCliente.Id, usuarioGrupo.Id));
        tercero.SetUsuario(usuarioCliente.Id, null);
        tercero.Actualizar(
            razonSocial: tercero.RazonSocial,
            nombreFantasia: tercero.NombreFantasia,
            condicionIvaId: tercero.CondicionIvaId,
            telefono: tercero.Telefono,
            celular: tercero.Celular,
            email: tercero.Email,
            web: tercero.Web,
            domicilio: tercero.Domicilio,
            nroIngresosBrutos: tercero.NroIngresosBrutos,
            nroMunicipal: tercero.NroMunicipal,
            limiteCredito: tercero.LimiteCredito,
            porcentajeMaximoDescuento: tercero.PorcentajeMaximoDescuento,
            vigenciaCreditoDesde: tercero.VigenciaCreditoDesde,
            vigenciaCreditoHasta: tercero.VigenciaCreditoHasta,
            facturable: tercero.Facturable,
            cobradorId: cobrador.Id,
            aplicaComisionCobrador: false,
            pctComisionCobrador: 0m,
            vendedorId: vendedor.Id,
            aplicaComisionVendedor: false,
            pctComisionVendedor: 0m,
            observacion: tercero.Observacion,
            userId: null);
        await db.SaveChangesAsync();

        db.MediosContacto.Add(MedioContacto.Crear(tercero.Id, "contacto@cliente.com", tipoMedioContactoId: 2, orden: 0, esDefecto: true, observacion: "Principal"));

        var ejercicio = Ejercicio.Crear(
            "Ejercicio vigente",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));
        db.Ejercicios.Add(ejercicio);
        await db.SaveChangesAsync();

        var planCuenta = PlanCuenta.Crear(ejercicio.Id, null, "1.01.001", "Deudores por ventas", 3, "001", true, null, 'D');
        db.PlanCuentas.Add(planCuenta);
        await db.SaveChangesAsync();

        db.PlanCuentasParametros.Add(PlanCuentaParametro.Crear(ejercicio.Id, planCuenta.Id, "PERSONAS", tercero.Id));
        await db.SaveChangesAsync();

        var repo = Substitute.For<ITerceroRepository>();
        repo.GetByIdAsync(tercero.Id, Arg.Any<CancellationToken>()).Returns(tercero);

        var handler = new GetTerceroByIdQueryHandler(repo, db, mapper, NullLogger<GetTerceroByIdQueryHandler>.Instance);

        var result = await handler.Handle(new GetTerceroByIdQuery(tercero.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CuentaContableCodigo.Should().Be("1.01.001");
        result.Value.CuentaContableDescripcion.Should().Be("Deudores por ventas");
        result.Value.MediosContacto.Should().ContainSingle();
        result.Value.MediosContacto[0].Valor.Should().Be("contacto@cliente.com");
        result.Value.MediosContacto[0].TipoInferidoCodigo.Should().Be("EMAIL");
        result.Value.MediosContacto[0].TipoInferidoDescripcion.Should().Be("Correo electrónico");
        result.Value.MediosContacto[0].EsDefecto.Should().BeTrue();
        result.Value.EstadoVisibleDescripcion.Should().Be(result.Value.EstadoOperativoDescripcion);
        result.Value.EstadoVisibleBloquea.Should().Be(result.Value.EstadoOperativoBloquea);
        result.Value.CobradorUserName.Should().Be("cobra-uno");
        result.Value.CobradorNombre.Should().Be("Cobrador Uno");
        result.Value.VendedorUserName.Should().Be("vende-uno");
        result.Value.VendedorNombre.Should().Be("Vendedor Uno");
        result.Value.AccesoUsuarioCliente.Should().BeTrue();
        result.Value.UsuarioClienteUserName.Should().Be("cliente-web");
        result.Value.UsuarioClienteGrupoUserName.Should().Be("grupo-clientes");
        result.Value.UsuarioCliente.Should().NotBeNull();
        result.Value.UsuarioCliente!.TienePasswordConfigurada.Should().BeTrue();
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static TipoDocumento CreateTipoDocumento(long id, short codigo, string descripcion)
    {
        var entity = (TipoDocumento)Activator.CreateInstance(typeof(TipoDocumento), nonPublic: true)!;
        typeof(TipoDocumento).GetProperty(nameof(TipoDocumento.Id))!.SetValue(entity, id);
        typeof(TipoDocumento).GetProperty(nameof(TipoDocumento.Codigo))!.SetValue(entity, codigo);
        typeof(TipoDocumento).GetProperty(nameof(TipoDocumento.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }

    private static CondicionIva CreateCondicionIva(long id, short codigo, string descripcion)
    {
        var entity = (CondicionIva)Activator.CreateInstance(typeof(CondicionIva), nonPublic: true)!;
        typeof(CondicionIva).GetProperty(nameof(CondicionIva.Id))!.SetValue(entity, id);
        typeof(CondicionIva).GetProperty(nameof(CondicionIva.Codigo))!.SetValue(entity, codigo);
        typeof(CondicionIva).GetProperty(nameof(CondicionIva.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }
}
