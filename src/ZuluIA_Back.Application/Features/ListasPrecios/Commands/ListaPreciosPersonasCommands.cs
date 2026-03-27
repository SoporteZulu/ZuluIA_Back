using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public record AddPersonaAListaPreciosCommand(long ListaId, long PersonaId) : IRequest<Result<long>>;

public record RemovePersonaDeListaPreciosCommand(long ListaId, long PersonaId) : IRequest<Result>;

public class AddPersonaAListaPreciosCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddPersonaAListaPreciosCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddPersonaAListaPreciosCommand request, CancellationToken ct)
    {
        var existe = await db.ListasPreciosPersonas
            .AnyAsync(x => x.ListaPreciosId == request.ListaId && x.PersonaId == request.PersonaId, ct);
        if (existe)
            return Result.Failure<long>("La persona ya está asignada a esta lista.");

        ListaPrecioPersona rel;
        try
        {
            rel = ListaPrecioPersona.Crear(request.ListaId, request.PersonaId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.ListasPreciosPersonas.Add(rel);
        await db.SaveChangesAsync(ct);
        return Result.Success(rel.Id);
    }
}

public class RemovePersonaDeListaPreciosCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RemovePersonaDeListaPreciosCommand, Result>
{
    public async Task<Result> Handle(RemovePersonaDeListaPreciosCommand request, CancellationToken ct)
    {
        var rel = await db.ListasPreciosPersonas
            .FirstOrDefaultAsync(x => x.ListaPreciosId == request.ListaId && x.PersonaId == request.PersonaId, ct);
        if (rel is null)
            return Result.Failure("La persona no está asignada a esta lista.");

        db.ListasPreciosPersonas.Remove(rel);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AddPersonaAListaPreciosCommandValidator : AbstractValidator<AddPersonaAListaPreciosCommand>
{
    public AddPersonaAListaPreciosCommandValidator()
    {
        RuleFor(x => x.ListaId).GreaterThan(0);
        RuleFor(x => x.PersonaId).GreaterThan(0);
    }
}

public class RemovePersonaDeListaPreciosCommandValidator : AbstractValidator<RemovePersonaDeListaPreciosCommand>
{
    public RemovePersonaDeListaPreciosCommandValidator()
    {
        RuleFor(x => x.ListaId).GreaterThan(0);
        RuleFor(x => x.PersonaId).GreaterThan(0);
    }
}
