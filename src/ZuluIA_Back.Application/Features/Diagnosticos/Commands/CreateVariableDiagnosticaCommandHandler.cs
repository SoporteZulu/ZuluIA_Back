using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreateVariableDiagnosticaCommandHandler(
    IApplicationDbContext db,
    IRepository<VariableDiagnostica> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateVariableDiagnosticaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateVariableDiagnosticaCommand request, CancellationToken ct)
    {
        if (!await db.AspectosDiagnostico.AsNoTracking().AnyAsync(x => x.Id == request.AspectoId, ct))
            return Result.Failure<long>($"No se encontró el aspecto diagnóstico ID {request.AspectoId}.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.VariablesDiagnosticas.AsNoTracking().AnyAsync(x => x.AspectoId == request.AspectoId && x.Codigo == codigo, ct))
            return Result.Failure<long>($"Ya existe una variable con código '{request.Codigo}' para el aspecto indicado.");

        if (request.Tipo != TipoVariableDiagnostica.Opcion && request.Opciones?.Any() == true)
            return Result.Failure<long>("Solo las variables de tipo opción pueden registrar opciones.");

        var variable = VariableDiagnostica.Crear(request.AspectoId, request.Codigo, request.Descripcion, request.Tipo, request.Requerida, request.Peso, currentUser.UserId);
        foreach (var opcion in request.Opciones ?? [])
            variable.AgregarOpcion(VariableDiagnosticaOpcion.Crear(0, opcion.Codigo, opcion.Descripcion, opcion.ValorNumerico, opcion.Orden));

        await repo.AddAsync(variable, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(variable.Id);
    }
}
