using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Application.Features.Usuarios.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateUsuarioCommandHandler ───────────────────────────────────────────────

public class CreateUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateUsuarioCommandHandler Sut() => new(_repo, _uow, _user);

    private static CreateUsuarioCommand ValidCommand() => new(
        "jdoe", "John Doe", "jdoe@test.com", null, new List<long> { 1L });

    [Fact]
    public async Task Handle_UserNameDuplicado_RetornaFailure()
    {
        _repo.ExisteUserNameAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(true);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("usuario");
    }

    [Fact]
    public async Task Handle_DatosValidos_CreaUsuarioYRetornaId()
    {
        _repo.ExisteUserNameAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── UpdateUsuarioCommandHandler ───────────────────────────────────────────────

public class UpdateUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private UpdateUsuarioCommandHandler Sut() => new(_repo, _uow, _user);

    private static UpdateUsuarioCommand ValidCommand(long id = 1) => new(
        id, "John Doe Updated", "jdoe@test.com", new List<long> { 1L });

    [Fact]
    public async Task Handle_UsuarioNoExiste_RetornaFailure()
    {
        _repo.GetByIdConSucursalesAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Usuario?)null);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UsuarioExiste_ActualizaYRetornaSuccess()
    {
        var usuario = Usuario.Crear("jdoe", "John Doe", null, null, null);
        _repo.GetByIdConSucursalesAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(ValidCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Usuario>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── DeleteUsuarioCommandHandler ───────────────────────────────────────────────

public class DeleteUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private DeleteUsuarioCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_UsuarioNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Usuario?)null);

        var result = await Sut().Handle(new DeleteUsuarioCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UsuarioExiste_DesactivaYPersiste()
    {
        var usuario = Usuario.Crear("jdoe", "John Doe", null, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new DeleteUsuarioCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Usuario>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── DeleteMenuItemCommandHandler ─────────────────────────────────────────────

public class DeleteMenuItemCommandHandlerTests
{
    private readonly IMenuRepository _repo = Substitute.For<IMenuRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private DeleteMenuItemCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((MenuItem?)null);

        var result = await Sut().Handle(new DeleteMenuItemCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No se encontro");
    }

    [Fact]
    public async Task Handle_ItemExiste_DesactivaYPersiste()
    {
        var item = MenuItem.Crear(null, "Clientes", "frmClientes", "icon", 1, 1);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(item);

        var result = await Sut().Handle(new DeleteMenuItemCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.Activo.Should().BeFalse();
        _repo.Received(1).Update(item);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── ActivateMenuItemCommandHandler ───────────────────────────────────────────

public class ActivateMenuItemCommandHandlerTests
{
    private readonly IMenuRepository _repo = Substitute.For<IMenuRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private ActivateMenuItemCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((MenuItem?)null);

        var result = await Sut().Handle(new ActivateMenuItemCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No se encontro");
    }

    [Fact]
    public async Task Handle_ItemExiste_ActivaYPersiste()
    {
        var item = MenuItem.Crear(null, "Clientes", "frmClientes", "icon", 1, 1);
        item.Desactivar();
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(item);

        var result = await Sut().Handle(new ActivateMenuItemCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.Activo.Should().BeTrue();
        _repo.Received(1).Update(item);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── Menu lifecycle validators ────────────────────────────────────────────────

public class MenuLifecycleCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeleteValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new DeleteMenuItemCommandValidator();

        var result = validator.Validate(new DeleteMenuItemCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void DeleteValidator_IdValido_Pasa()
    {
        var validator = new DeleteMenuItemCommandValidator();

        var result = validator.Validate(new DeleteMenuItemCommand(1));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ActivateValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new ActivateMenuItemCommandValidator();

        var result = validator.Validate(new ActivateMenuItemCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void ActivateValidator_IdValido_Pasa()
    {
        var validator = new ActivateMenuItemCommandValidator();

        var result = validator.Validate(new ActivateMenuItemCommand(1));

        result.IsValid.Should().BeTrue();
    }
}

// ── GetUsuarioByIdQueryHandler ────────────────────────────────────────────────

public class GetUsuarioByIdQueryHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetUsuarioByIdQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_UsuarioNoExiste_RetornaNull()
    {
        _repo.GetByIdConSucursalesAsync(99, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var result = await Sut().Handle(new GetUsuarioByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UsuarioExiste_RetornaMappedDto()
    {
        var usuario = Usuario.Crear("jdoe", "John Doe", null, null, null);
        var dto = new UsuarioDto { Id = 1, UserName = "jdoe" };
        _repo.GetByIdConSucursalesAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);
        _mapper.Map<UsuarioDto>(usuario).Returns(dto);

        var result = await Sut().Handle(new GetUsuarioByIdQuery(1), CancellationToken.None);

        result.Should().BeSameAs(dto);
    }
}

// ── GetUsuariosPagedQueryHandler ──────────────────────────────────────────────

public class GetUsuariosPagedQueryHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetUsuariosPagedQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_SinUsuarios_RetornaPaginaVacia()
    {
        var empty = new PagedResult<Usuario>([], 1, 10, 0);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                            Arg.Any<bool?>(), Arg.Any<CancellationToken>())
             .Returns(empty);
        _mapper.Map<IReadOnlyList<UsuarioListDto>>(Arg.Any<IReadOnlyList<Usuario>>())
               .Returns(new List<UsuarioListDto>().AsReadOnly());

        var result = await Sut().Handle(new GetUsuariosPagedQuery(1, 10, null, null), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ConUsuarios_RetornaPaginaConItems()
    {
        var usuarios = new List<Usuario> { Usuario.Crear("jdoe", null, null, null, null) };
        var paged = new PagedResult<Usuario>(usuarios, 1, 10, 1);
        var dtos = new List<UsuarioListDto> { new() { Id = 1, UserName = "jdoe" } }.AsReadOnly();

        _repo.GetPagedAsync(1, 10, null, null, Arg.Any<CancellationToken>()).Returns(paged);
        _mapper.Map<IReadOnlyList<UsuarioListDto>>(Arg.Any<IReadOnlyList<Usuario>>()).Returns(dtos);

        var result = await Sut().Handle(new GetUsuariosPagedQuery(1, 10, null, null), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }
}

// ── SetParametroUsuarioCommandHandler ────────────────────────────────────────

public class SetParametroUsuarioCommandHandlerTests
{
    private readonly IParametroUsuarioRepository _repo = Substitute.For<IParametroUsuarioRepository>();
    private SetParametroUsuarioCommandHandler Sut() => new(_repo);

    [Fact]
    public async Task Handle_ClaveVacia_RetornaFailure()
    {
        var result = await Sut().Handle(new SetParametroUsuarioCommand(1, "", "valor"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("clave");
    }

    [Fact]
    public async Task Handle_ClaveValida_UpsertYRetornaSuccess()
    {
        var result = await Sut().Handle(new SetParametroUsuarioCommand(1, "TEMA", "oscuro"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpsertAsync(1, "TEMA", "oscuro", Arg.Any<CancellationToken>());
    }
}

// ── SetMenuUsuarioCommandHandler ──────────────────────────────────────────────

public class SetMenuUsuarioCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private SetMenuUsuarioCommandHandler Sut() => new(_db, _uow);

    [Fact]
    public async Task Handle_SinMenuPrevio_AgregaNuevosYRetornaSuccess()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<MenuUsuario>();
        _db.MenuUsuario.Returns(mockDbSet);

        var cmd = new SetMenuUsuarioCommand(1, new List<long> { 10L, 20L });
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── SetPermisoUsuarioCommandHandler ──────────────────────────────────────────

public class SetPermisoUsuarioCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private SetPermisoUsuarioCommandHandler Sut() => new(_db, _uow);

    [Fact]
    public async Task Handle_SinPermisoExistente_CreaYRetornaSuccess()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<SeguridadUsuario>();
        _db.SeguridadUsuario.Returns(mockDbSet);

        var cmd = new SetPermisoUsuarioCommand(1, 5, true);
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConPermisoExistente_ActualizaYRetornaSuccess()
    {
        var existing = SeguridadUsuario.Crear(5, 1, false);
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<SeguridadUsuario>(new[] { existing });
        _db.SeguridadUsuario.Returns(mockDbSet);

        var cmd = new SetPermisoUsuarioCommand(1, 5, true);
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existing.Valor.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetMenuUsuarioQueryHandler ────────────────────────────────────────────────

public class GetMenuUsuarioQueryHandlerTests
{
    private readonly IMenuRepository _menuRepo = Substitute.For<IMenuRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetMenuUsuarioQueryHandler Sut() => new(_menuRepo, _mapper);

    [Fact]
    public async Task Handle_SinMenu_RetornaListaVacia()
    {
        _menuRepo.GetMenuUsuarioAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
                 .Returns(new List<MenuItem>().AsReadOnly());
        _mapper.Map<IReadOnlyList<MenuItemDto>>(Arg.Any<IReadOnlyList<MenuItem>>())
               .Returns(new List<MenuItemDto>().AsReadOnly());

        var result = await Sut().Handle(new GetMenuUsuarioQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetPermisosUsuarioQueryHandler ────────────────────────────────────────────

public class GetPermisosUsuarioQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetPermisosUsuarioQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_SinPermisos_RetornaListaVacia()
    {
        var mockSeguridad77 = MockDbSetHelper.CreateMockDbSet<Seguridad>();
        _db.Seguridad.Returns(mockSeguridad77);
        var mockSeguridadUsuario78 = MockDbSetHelper.CreateMockDbSet<SeguridadUsuario>();
        _db.SeguridadUsuario.Returns(mockSeguridadUsuario78);

        var result = await Sut().Handle(
            new GetPermisosUsuarioQuery(1L),
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}
