using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class CreateSeguridadCommandHandler(
    IRepository<Seguridad> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateSeguridadCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateSeguridadCommand request, CancellationToken ct)
    {
        var identificador = request.Identificador.Trim().ToUpperInvariant();
        var existe = await repo.ExistsAsync(x => x.Identificador == identificador, ct);
        if (existe)
            return Result.Failure<long>($"Ya existe un permiso con el identificador '{request.Identificador}'.");

        Seguridad seguridad;
        try
        {
            seguridad = Seguridad.Crear(identificador, request.Descripcion, request.AplicaSeguridadPorUsuario);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(seguridad, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(seguridad.Id);
    }
}
