using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public record CreateInventarioConteoCommand(
    long UsuarioId,
    DateTimeOffset FechaApertura,
    int NroAuditoria) : IRequest<Result<long>>;

public record CerrarInventarioConteoCommand(
    long Id,
    DateTimeOffset FechaCierre) : IRequest<Result>;

public class CreateInventarioConteoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateInventarioConteoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateInventarioConteoCommand request, CancellationToken ct)
    {
        var nroAuditoriaExists = await db.InventariosConteo
            .AnyAsync(x => x.NroAuditoria == request.NroAuditoria, ct);

        if (nroAuditoriaExists)
            return Result.Failure<long>($"Ya existe un inventario para la auditoria {request.NroAuditoria}.");

        InventarioConteo inv;
        try
        {
            inv = InventarioConteo.Crear(request.UsuarioId, request.FechaApertura, request.NroAuditoria);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.InventariosConteo.Add(inv);
        await db.SaveChangesAsync(ct);

        return Result.Success(inv.Id);
    }
}

public class CerrarInventarioConteoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CerrarInventarioConteoCommand, Result>
{
    public async Task<Result> Handle(CerrarInventarioConteoCommand request, CancellationToken ct)
    {
        var inv = await db.InventariosConteo.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (inv is null)
            return Result.Failure($"Inventario {request.Id} no encontrado.");

        try
        {
            inv.Cerrar(request.FechaCierre);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateInventarioConteoCommandValidator : AbstractValidator<CreateInventarioConteoCommand>
{
    public CreateInventarioConteoCommandValidator()
    {
        RuleFor(x => x.UsuarioId).GreaterThan(0);
        RuleFor(x => x.NroAuditoria).GreaterThan(0);
    }
}

public class CerrarInventarioConteoCommandValidator : AbstractValidator<CerrarInventarioConteoCommand>
{
    public CerrarInventarioConteoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
