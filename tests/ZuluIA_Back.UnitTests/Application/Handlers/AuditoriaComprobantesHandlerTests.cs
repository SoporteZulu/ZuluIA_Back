using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Auditoria.Commands;
using ZuluIA_Back.Application.Features.Auditoria.Queries;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class RegistrarAuditoriaComprobanteCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly RegistrarAuditoriaComprobanteCommandHandler _handler;

    public RegistrarAuditoriaComprobanteCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new RegistrarAuditoriaComprobanteCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_ComandoValido_RegistraAuditoriaYPersiste()
    {
        var auditorias = MockDbSetHelper.CreateMockDbSet<AuditoriaComprobante>();
        _db.AuditoriaComprobantes.Returns(auditorias);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(
            new RegistrarAuditoriaComprobanteCommand(25, 7, AccionAuditoria.AfipSolicitud, "solicitud enviada", "127.0.0.1"),
            CancellationToken.None);

        result.Should().Be(Unit.Value);
        auditorias.Received(1).Add(Arg.Is<AuditoriaComprobante>(x =>
            x.ComprobanteId == 25 &&
            x.UsuarioId == 7 &&
            x.Accion == AccionAuditoria.AfipSolicitud &&
            x.DetalleCambio == "solicitud enviada" &&
            x.IpOrigen == "127.0.0.1"));
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class RegistrarAuditoriaComprobanteCommandValidatorTests
{
    [Fact]
    public void Validate_ComprobanteIdInvalido_ReportaError()
    {
        var validator = new RegistrarAuditoriaComprobanteCommandValidator();

        var result = validator.Validate(new RegistrarAuditoriaComprobanteCommand(0, 1, AccionAuditoria.Creado, null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegistrarAuditoriaComprobanteCommand.ComprobanteId));
    }
}

public class GetAuditoriaComprobanteQueryHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly GetAuditoriaComprobanteQueryHandler _handler;

    public GetAuditoriaComprobanteQueryHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new GetAuditoriaComprobanteQueryHandler(_db);
    }

    [Fact]
    public async Task Handle_CuandoHayRegistros_RetornaSoloElComprobanteSolicitado()
    {
        var registros = MockDbSetHelper.CreateMockDbSet(new[]
        {
            AuditoriaComprobante.Registrar(10, 2, AccionAuditoria.Anulado, "segundo", null),
            AuditoriaComprobante.Registrar(10, 1, AccionAuditoria.Creado, "primero", null),
            AuditoriaComprobante.Registrar(11, 3, AccionAuditoria.AfipError, "otro", null)
        });
        _db.AuditoriaComprobantes.Returns(registros);

        var result = await _handler.Handle(new GetAuditoriaComprobanteQuery(10), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.ComprobanteId).Distinct().Should().ContainSingle().Which.Should().Be(10);
        result.Select(x => x.Accion).Should().BeEquivalentTo([nameof(AccionAuditoria.Anulado), nameof(AccionAuditoria.Creado)]);
    }
}