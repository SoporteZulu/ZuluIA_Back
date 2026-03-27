using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Sucursales.Commands;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;
using ZuluIA_Back.Application.Features.Sucursales.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateSucursalCommandHandler ─────────────────────────────────────────────

public class CreateSucursalCommandHandlerTests
{
    private readonly ISucursalRepository _repo = Substitute.For<ISucursalRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateSucursalCommandHandler Sut() => new(_repo, _uow, _user);

    private static CreateSucursalCommand ValidCommand() => new(
        "Empresa SA", null, "30-11111111-1", null, 1, 1, 1,
        null, null, null, null, null, null, null,
        null, null, null, null, null, null, 443, false);

    [Fact]
    public async Task Handle_CuitDuplicado_RetornaFailure()
    {
        _repo.ExisteCuitAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(true);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("CUIT");
    }

    [Fact]
    public async Task Handle_DatosValidos_CreaSucursalYRetornaId()
    {
        _repo.ExisteCuitAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Sucursal>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── UpdateSucursalCommandHandler ─────────────────────────────────────────────

public class UpdateSucursalCommandHandlerTests
{
    private readonly ISucursalRepository _repo = Substitute.For<ISucursalRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private UpdateSucursalCommandHandler Sut() => new(_repo, _uow, _user);

    private static UpdateSucursalCommand ValidCommand(long id = 1) => new(
        id, "Empresa SA", null, "30-11111111-1", null, 1, 1, 1,
        null, null, null, null, null, null, null,
        null, null, null, null, null, null, 443, false);

    [Fact]
    public async Task Handle_SucursalNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Sucursal?)null);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CuitDuplicadoDeOtra_RetornaFailure()
    {
        var sucursal = Sucursal.Crear("Empresa SA", "30-11111111-1", 1, 1, 1, false, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(sucursal);
        _repo.ExisteCuitAsync("30-11111111-1", 1, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Sut().Handle(ValidCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("CUIT");
    }

    [Fact]
    public async Task Handle_DatosValidos_ActualizaYRetornaSuccess()
    {
        var sucursal = Sucursal.Crear("Empresa SA", "30-11111111-1", 1, 1, 1, false, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(sucursal);
        _repo.ExisteCuitAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(ValidCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Sucursal>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── DeleteSucursalCommandHandler ─────────────────────────────────────────────

public class DeleteSucursalCommandHandlerTests
{
    private readonly ISucursalRepository _repo = Substitute.For<ISucursalRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private DeleteSucursalCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_SucursalNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Sucursal?)null);

        var result = await Sut().Handle(new DeleteSucursalCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SucursalExiste_DesactivaYPersiste()
    {
        var sucursal = Sucursal.Crear("Empresa SA", "30-11111111-1", 1, 1, 1, false, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(sucursal);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new DeleteSucursalCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Sucursal>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetSucursalByIdQueryHandler ───────────────────────────────────────────────

public class GetSucursalByIdQueryHandlerTests
{
    private readonly ISucursalRepository _repo = Substitute.For<ISucursalRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetSucursalByIdQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_SucursalNoExiste_RetornaNull()
    {
        _repo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Sucursal?)null);

        var result = await Sut().Handle(new GetSucursalByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SucursalExiste_RetornaMappedDto()
    {
        var sucursal = Sucursal.Crear("Empresa SA", "30-11111111-1", 1, 1, 1, false, null);
        var dto = new SucursalDto { Id = 1, RazonSocial = "Empresa SA" };
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(sucursal);
        _mapper.Map<SucursalDto>(sucursal).Returns(dto);

        var result = await Sut().Handle(new GetSucursalByIdQuery(1), CancellationToken.None);

        result.Should().BeSameAs(dto);
    }
}

// ── GetSucursalesQueryHandler ─────────────────────────────────────────────────

public class GetSucursalesQueryHandlerTests
{
    private readonly ISucursalRepository _repo = Substitute.For<ISucursalRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetSucursalesQueryHandler Sut() => new(_mapper, _repo);

    [Fact]
    public async Task Handle_SoloActivas_RetornaListaMapeada()
    {
        var sucursales = new List<Sucursal>
        {
            Sucursal.Crear("SA1", "20-11111111-1", 1, 1, 1, false, null),
            Sucursal.Crear("SA2", "20-22222222-2", 1, 1, 1, false, null)
        }.AsReadOnly();
        var dtos = new List<SucursalListDto>
        {
            new() { Id = 1, RazonSocial = "SA1" },
            new() { Id = 2, RazonSocial = "SA2" }
        }.AsReadOnly();

        _repo.GetAllActivasAsync(Arg.Any<CancellationToken>()).Returns(sucursales);
        _mapper.Map<IReadOnlyList<SucursalListDto>>(sucursales).Returns(dtos);

        var result = await Sut().Handle(new GetSucursalesQuery(SoloActivas: true), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_TodasSucursales_UsaGetAllAsync()
    {
        var sucursales = new List<Sucursal>().AsReadOnly();
        var dtos = new List<SucursalListDto>().AsReadOnly();

        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(sucursales);
        _mapper.Map<IReadOnlyList<SucursalListDto>>(sucursales).Returns(dtos);

        var result = await Sut().Handle(new GetSucursalesQuery(SoloActivas: false), CancellationToken.None);

        await _repo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        result.Should().BeEmpty();
    }
}
