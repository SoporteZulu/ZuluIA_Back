using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;
using ZuluIA_Back.Application.Features.DescuentosComerciales.DTOs;
using ZuluIA_Back.Application.Features.DescuentosComerciales.Queries;
using ZuluIA_Back.Application.Features.ListasPrecios.Commands;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Application.Features.ListasPrecios.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateDescuentoComercialCommandHandler ────────────────────────────────────

public class CreateDescuentoComercialCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateDescuentoComercialCommandHandler Sut() => new(_db, _uow, _user);

    [Fact]
    public async Task Handle_DatosValidos_CreaDescuentoYRetornaId()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<DescuentoComercial>();
        _db.DescuentosComerciales.Returns(mockDbSet);
        _user.UserId.Returns((long?)1L);

        var cmd = new CreateDescuentoComercialCommand(
            1, 1, DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), 10m);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PorcentajeInvalido_RetornaFailure()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<DescuentoComercial>();
        _db.DescuentosComerciales.Returns(mockDbSet);

        var cmd = new CreateDescuentoComercialCommand(
            1, 1, DateOnly.FromDateTime(DateTime.Today), null, 150m); // > 100%

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

// ── UpdateDescuentoComercialCommandHandler ────────────────────────────────────

public class UpdateDescuentoComercialCommandHandlerTests
{
    private readonly IRepository<DescuentoComercial> _repo = Substitute.For<IRepository<DescuentoComercial>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private UpdateDescuentoComercialCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_DescuentoNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((DescuentoComercial?)null);

        var cmd = new UpdateDescuentoComercialCommand(
            99, DateOnly.FromDateTime(DateTime.Today), null, 10m);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DatosValidos_ActualizaYRetornaSuccess()
    {
        var descuento = DescuentoComercial.Crear(
            1, 1, DateOnly.FromDateTime(DateTime.Today), null, 10m, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(descuento);
        _user.UserId.Returns((long?)1L);

        var cmd = new UpdateDescuentoComercialCommand(
            1, DateOnly.FromDateTime(DateTime.Today), null, 15m);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<DescuentoComercial>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── DeleteDescuentoComercialCommandHandler ────────────────────────────────────

public class DeleteDescuentoComercialCommandHandlerTests
{
    private readonly IRepository<DescuentoComercial> _repo = Substitute.For<IRepository<DescuentoComercial>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private DeleteDescuentoComercialCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_DescuentoNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((DescuentoComercial?)null);

        var result = await Sut().Handle(new DeleteDescuentoComercialCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DescuentoExiste_EliminaYPersiste()
    {
        var descuento = DescuentoComercial.Crear(
            1, 1, DateOnly.FromDateTime(DateTime.Today), null, 10m, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(descuento);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new DeleteDescuentoComercialCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<DescuentoComercial>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetDescuentosComercialesQueryHandler ──────────────────────────────────────

public class GetDescuentosComercialesQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetDescuentosComercialesQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_SinDescuentos_RetornaListaVacia()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<DescuentoComercial>();
        _db.DescuentosComerciales.Returns(mockDbSet);

        var result = await Sut().Handle(
            new GetDescuentosComercialesQuery(null, null, null),
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── CreateListaPreciosCommandHandler ─────────────────────────────────────────

public class CreateListaPreciosCommandHandlerTests
{
    private readonly IListaPreciosRepository _repo = Substitute.For<IListaPreciosRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateListaPreciosCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_DatosValidos_CreaListaYRetornaId()
    {
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(
            new CreateListaPreciosCommand("Lista Test", 1, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<ListaPrecios>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── UpdateListaPreciosCommandHandler ─────────────────────────────────────────

public class UpdateListaPreciosCommandHandlerTests
{
    private readonly IListaPreciosRepository _repo = Substitute.For<IListaPreciosRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private UpdateListaPreciosCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_ListaNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((ListaPrecios?)null);

        var result = await Sut().Handle(
            new UpdateListaPreciosCommand(99, "Test", 1, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ListaExiste_ActualizaYRetornaSuccess()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1, null, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(lista);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(
            new UpdateListaPreciosCommand(1, "Lista Actualizada", 1, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<ListaPrecios>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── DeleteListaPreciosCommandHandler ─────────────────────────────────────────

public class DeleteListaPreciosCommandHandlerTests
{
    private readonly IListaPreciosRepository _repo = Substitute.For<IListaPreciosRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private DeleteListaPreciosCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_ListaNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((ListaPrecios?)null);

        var result = await Sut().Handle(new DeleteListaPreciosCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ListaExiste_DesactivaYPersiste()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1, null, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(lista);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new DeleteListaPreciosCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<ListaPrecios>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── UpsertItemEnListaCommandHandler ──────────────────────────────────────────

public class UpsertItemEnListaCommandHandlerTests
{
    private readonly IListaPreciosRepository _repo = Substitute.For<IListaPreciosRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private UpsertItemEnListaCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_ListaNoExiste_RetornaFailure()
    {
        _repo.GetByIdConItemsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((ListaPrecios?)null);

        var result = await Sut().Handle(new UpsertItemEnListaCommand(99, 1, 100m, 0m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ListaExiste_UpsertItemYPersiste()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1, null, null, null);
        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(lista);

        var result = await Sut().Handle(new UpsertItemEnListaCommand(1, 5, 100m, 0m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<ListaPrecios>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── RemoveItemDeListaCommandHandler ──────────────────────────────────────────

public class RemoveItemDeListaCommandHandlerTests
{
    private readonly IListaPreciosRepository _repo = Substitute.For<IListaPreciosRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private RemoveItemDeListaCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_ListaNoExiste_RetornaFailure()
    {
        _repo.GetByIdConItemsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((ListaPrecios?)null);

        var result = await Sut().Handle(new RemoveItemDeListaCommand(99, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

// ── GetListasPreciosQueryHandler ──────────────────────────────────────────────

public class GetListasPreciosQueryHandlerTests
{
    private readonly IListaPreciosRepository _repo = Substitute.For<IListaPreciosRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetListasPreciosQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_RetornaListaMapeada()
    {
        var listas = new List<ListaPrecios>().AsReadOnly();
        var dtos = new List<ListaPreciosDto>().AsReadOnly();
        _repo.GetActivasAsync(Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>()).Returns(listas);
        _mapper.Map<IReadOnlyList<ListaPreciosDto>>(listas).Returns(dtos);

        var result = await Sut().Handle(new GetListasPreciosQuery(null), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetListaPreciosByIdQueryHandler ───────────────────────────────────────────

public class GetListaPreciosByIdQueryHandlerTests
{
    private readonly IListaPreciosRepository _repo = Substitute.For<IListaPreciosRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetListaPreciosByIdQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ListaNoExiste_RetornaNull()
    {
        _repo.GetByIdConItemsAsync(99, Arg.Any<CancellationToken>())
             .Returns((ListaPrecios?)null);

        var result = await Sut().Handle(new GetListaPreciosByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ListaExiste_RetornaMappedDto()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1, null, null, null);
        var dto = new ListaPreciosDetalleDto { Id = 1, Descripcion = "Lista Test" };
        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(lista);
        _mapper.Map<ListaPreciosDetalleDto>(lista).Returns(dto);

        var result = await Sut().Handle(new GetListaPreciosByIdQuery(1), CancellationToken.None);

        result.Should().BeSameAs(dto);
    }
}

// ── GetPrecioItemQueryHandler ─────────────────────────────────────────────────

public class GetPrecioItemQueryHandlerTests
{
    private readonly IListaPreciosRepository _repo = Substitute.For<IListaPreciosRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetPrecioItemQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaNull()
    {
        _repo.GetPrecioItemAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((ListaPreciosItem?)null);

        var result = await Sut().Handle(new GetPrecioItemQuery(1, 99), CancellationToken.None);

        result.Should().BeNull();
    }
}
