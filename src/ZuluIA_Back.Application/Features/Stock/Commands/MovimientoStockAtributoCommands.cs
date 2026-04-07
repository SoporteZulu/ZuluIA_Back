using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public record AddMovimientoStockAtributoCommand(
    long MovimientoStockId,
    long AtributoId,
    string Valor) : IRequest<Result<long>>;

public record UpdateMovimientoStockAtributoCommand(
    long MovimientoStockId,
    long MovimientoStockAtributoId,
    string Valor) : IRequest<Result>;

public record DeleteMovimientoStockAtributoCommand(
    long MovimientoStockId,
    long MovimientoStockAtributoId) : IRequest<Result>;

public class AddMovimientoStockAtributoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddMovimientoStockAtributoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddMovimientoStockAtributoCommand request, CancellationToken ct)
    {
        var exists = await db.MovimientosStock.AnyAsync(x => x.Id == request.MovimientoStockId, ct);
        if (!exists)
            return Result.Failure<long>($"Movimiento de stock {request.MovimientoStockId} no encontrado.");

        MovimientoStockAtributo atrib;
        try
        {
            atrib = MovimientoStockAtributo.Crear(request.MovimientoStockId, request.AtributoId, request.Valor);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.MovimientosStockAtributos.Add(atrib);
        await db.SaveChangesAsync(ct);

        return Result.Success(atrib.Id);
    }
}

public class UpdateMovimientoStockAtributoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateMovimientoStockAtributoCommand, Result>
{
    public async Task<Result> Handle(UpdateMovimientoStockAtributoCommand request, CancellationToken ct)
    {
        var atrib = await db.MovimientosStockAtributos
            .FirstOrDefaultAsync(
                x => x.Id == request.MovimientoStockAtributoId
                  && x.MovimientoStockId == request.MovimientoStockId,
                ct);

        if (atrib is null)
            return Result.Failure($"Atributo de movimiento {request.MovimientoStockAtributoId} no encontrado.");

        try
        {
            atrib.ActualizarValor(request.Valor);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteMovimientoStockAtributoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteMovimientoStockAtributoCommand, Result>
{
    public async Task<Result> Handle(DeleteMovimientoStockAtributoCommand request, CancellationToken ct)
    {
        var atrib = await db.MovimientosStockAtributos
            .FirstOrDefaultAsync(
                x => x.Id == request.MovimientoStockAtributoId
                  && x.MovimientoStockId == request.MovimientoStockId,
                ct);

        if (atrib is null)
            return Result.Failure($"Atributo de movimiento {request.MovimientoStockAtributoId} no encontrado.");

        db.MovimientosStockAtributos.Remove(atrib);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AddMovimientoStockAtributoCommandValidator : AbstractValidator<AddMovimientoStockAtributoCommand>
{
    public AddMovimientoStockAtributoCommandValidator()
    {
        RuleFor(x => x.MovimientoStockId).GreaterThan(0);
        RuleFor(x => x.AtributoId).GreaterThan(0);
        RuleFor(x => x.Valor).NotEmpty();
    }
}

public class UpdateMovimientoStockAtributoCommandValidator : AbstractValidator<UpdateMovimientoStockAtributoCommand>
{
    public UpdateMovimientoStockAtributoCommandValidator()
    {
        RuleFor(x => x.MovimientoStockId).GreaterThan(0);
        RuleFor(x => x.MovimientoStockAtributoId).GreaterThan(0);
        RuleFor(x => x.Valor).NotEmpty();
    }
}

public class DeleteMovimientoStockAtributoCommandValidator : AbstractValidator<DeleteMovimientoStockAtributoCommand>
{
    public DeleteMovimientoStockAtributoCommandValidator()
    {
        RuleFor(x => x.MovimientoStockId).GreaterThan(0);
        RuleFor(x => x.MovimientoStockAtributoId).GreaterThan(0);
    }
}
