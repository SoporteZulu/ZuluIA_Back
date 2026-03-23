using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record AddItemComponenteCommand(long ItemId, long ComponenteId, decimal Cantidad, long? UnidadMedidaId)
    : IRequest<Result<long>>;

public record UpdateItemComponenteCommand(long ItemId, long ComponenteIdRegistroId, decimal Cantidad, long? UnidadMedidaId)
    : IRequest<Result>;

public record DeleteItemComponenteCommand(long ItemId, long ComponenteIdRegistroId)
    : IRequest<Result>;

public class AddItemComponenteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddItemComponenteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddItemComponenteCommand request, CancellationToken ct)
    {
        var itemExiste = await db.Items.AnyAsync(x => x.Id == request.ItemId, ct);
        if (!itemExiste)
            return Result.Failure<long>("Item no encontrado.");

        var duplicado = await db.ItemsComponentes.AnyAsync(
            x => x.ItemPadreId == request.ItemId && x.ComponenteId == request.ComponenteId,
            ct);

        if (duplicado)
            return Result.Failure<long>("El componente ya esta asignado a este item.");

        ItemComponente componente;
        try
        {
            componente = ItemComponente.Crear(
                request.ItemId,
                request.ComponenteId,
                request.Cantidad,
                request.UnidadMedidaId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.ItemsComponentes.Add(componente);
        await db.SaveChangesAsync(ct);

        return Result.Success(componente.Id);
    }
}

public class UpdateItemComponenteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateItemComponenteCommand, Result>
{
    public async Task<Result> Handle(UpdateItemComponenteCommand request, CancellationToken ct)
    {
        var componente = await db.ItemsComponentes.FirstOrDefaultAsync(
            x => x.Id == request.ComponenteIdRegistroId && x.ItemPadreId == request.ItemId,
            ct);

        if (componente is null)
            return Result.Failure("Componente no encontrado.");

        try
        {
            componente.ActualizarCantidad(request.Cantidad, request.UnidadMedidaId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteItemComponenteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteItemComponenteCommand, Result>
{
    public async Task<Result> Handle(DeleteItemComponenteCommand request, CancellationToken ct)
    {
        var componente = await db.ItemsComponentes.FirstOrDefaultAsync(
            x => x.Id == request.ComponenteIdRegistroId && x.ItemPadreId == request.ItemId,
            ct);

        if (componente is null)
            return Result.Failure("Componente no encontrado.");

        db.ItemsComponentes.Remove(componente);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddUnidadManipulacionCommand(
    long ItemId,
    string Descripcion,
    decimal Cantidad,
    long UnidadMedidaId,
    long? TipoUnidadManipulacionId) : IRequest<Result<long>>;

public record UpdateUnidadManipulacionCommand(
    long ItemId,
    long UnidadManipulacionId,
    string Descripcion,
    decimal Cantidad,
    long UnidadMedidaId,
    long? TipoUnidadManipulacionId) : IRequest<Result>;

public record DeleteUnidadManipulacionCommand(long ItemId, long UnidadManipulacionId) : IRequest<Result>;

public class AddUnidadManipulacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddUnidadManipulacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddUnidadManipulacionCommand request, CancellationToken ct)
    {
        UnidadManipulacion unidad;
        try
        {
            unidad = UnidadManipulacion.Crear(
                request.ItemId,
                request.Descripcion,
                request.Cantidad,
                request.UnidadMedidaId,
                request.TipoUnidadManipulacionId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.UnidadesManipulacion.Add(unidad);
        await db.SaveChangesAsync(ct);

        return Result.Success(unidad.Id);
    }
}

public class UpdateUnidadManipulacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateUnidadManipulacionCommand, Result>
{
    public async Task<Result> Handle(UpdateUnidadManipulacionCommand request, CancellationToken ct)
    {
        var unidad = await db.UnidadesManipulacion.FirstOrDefaultAsync(
            x => x.Id == request.UnidadManipulacionId && x.ItemId == request.ItemId,
            ct);

        if (unidad is null)
            return Result.Failure("Unidad de manipulacion no encontrada.");

        try
        {
            unidad.Actualizar(
                request.Descripcion,
                request.Cantidad,
                request.UnidadMedidaId,
                request.TipoUnidadManipulacionId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteUnidadManipulacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteUnidadManipulacionCommand, Result>
{
    public async Task<Result> Handle(DeleteUnidadManipulacionCommand request, CancellationToken ct)
    {
        var unidad = await db.UnidadesManipulacion.FirstOrDefaultAsync(
            x => x.Id == request.UnidadManipulacionId && x.ItemId == request.ItemId,
            ct);

        if (unidad is null)
            return Result.Failure("Unidad de manipulacion no encontrada.");

        db.UnidadesManipulacion.Remove(unidad);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AddItemComponenteCommandValidator : AbstractValidator<AddItemComponenteCommand>
{
    public AddItemComponenteCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.ComponenteId).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0);
    }
}

public class UpdateItemComponenteCommandValidator : AbstractValidator<UpdateItemComponenteCommand>
{
    public UpdateItemComponenteCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.ComponenteIdRegistroId).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0);
    }
}

public class DeleteItemComponenteCommandValidator : AbstractValidator<DeleteItemComponenteCommand>
{
    public DeleteItemComponenteCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.ComponenteIdRegistroId).GreaterThan(0);
    }
}

public class AddUnidadManipulacionCommandValidator : AbstractValidator<AddUnidadManipulacionCommand>
{
    public AddUnidadManipulacionCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.UnidadMedidaId).GreaterThan(0);
    }
}

public class UpdateUnidadManipulacionCommandValidator : AbstractValidator<UpdateUnidadManipulacionCommand>
{
    public UpdateUnidadManipulacionCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.UnidadManipulacionId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.UnidadMedidaId).GreaterThan(0);
    }
}

public class DeleteUnidadManipulacionCommandValidator : AbstractValidator<DeleteUnidadManipulacionCommand>
{
    public DeleteUnidadManipulacionCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.UnidadManipulacionId).GreaterThan(0);
    }
}
