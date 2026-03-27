using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class CreateMenuItemCommandHandler(
    IMenuRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateMenuItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateMenuItemCommand request, CancellationToken ct)
    {
        var item = MenuItem.Crear(
            request.ParentId,
            request.Descripcion,
            request.Formulario,
            request.Icono,
            request.Nivel,
            request.Orden);

        await repo.AddAsync(item, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(item.Id);
    }
}