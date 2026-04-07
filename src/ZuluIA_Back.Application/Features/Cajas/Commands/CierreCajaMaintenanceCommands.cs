using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public record CreateCierreCajaCommand(
    DateTimeOffset FechaApertura,
    DateTimeOffset FechaCierre,
    long UsuarioId,
    int NroCierre) : IRequest<Result<long>>;

public record RegistrarControlTesoreriaCierreCajaCommand(long CierreCajaId, DateTimeOffset Fecha)
    : IRequest<Result>;

public record AddCierreCajaDetalleCommand(long CierreCajaId, long CajaCuentaBancariaId, decimal Diferencia)
    : IRequest<Result<long>>;

public record RemoveCierreCajaDetalleCommand(long CierreCajaId, long CierreCajaDetalleId)
    : IRequest<Result>;

public class CreateCierreCajaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCierreCajaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCierreCajaCommand request, CancellationToken ct)
    {
        CierreCaja cierre;
        try
        {
            cierre = CierreCaja.Crear(request.FechaApertura, request.FechaCierre, request.UsuarioId, request.NroCierre);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.CierresCaja.Add(cierre);
        await db.SaveChangesAsync(ct);

        return Result.Success(cierre.Id);
    }
}

public class RegistrarControlTesoreriaCierreCajaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegistrarControlTesoreriaCierreCajaCommand, Result>
{
    public async Task<Result> Handle(RegistrarControlTesoreriaCierreCajaCommand request, CancellationToken ct)
    {
        var cierre = await db.CierresCaja.FindAsync([request.CierreCajaId], ct);
        if (cierre is null)
            return Result.Failure($"Cierre de caja {request.CierreCajaId} no encontrado.");

        cierre.RegistrarControlTesoreria(request.Fecha);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AddCierreCajaDetalleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddCierreCajaDetalleCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddCierreCajaDetalleCommand request, CancellationToken ct)
    {
        var existe = await db.CierresCaja.AnyAsync(c => c.Id == request.CierreCajaId, ct);
        if (!existe)
            return Result.Failure<long>($"Cierre de caja {request.CierreCajaId} no encontrado.");

        CierreCajaDetalle detalle;
        try
        {
            detalle = CierreCajaDetalle.Crear(request.CierreCajaId, request.CajaCuentaBancariaId, request.Diferencia);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.CierresCajaDetalle.Add(detalle);
        await db.SaveChangesAsync(ct);

        return Result.Success(detalle.Id);
    }
}

public class RemoveCierreCajaDetalleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RemoveCierreCajaDetalleCommand, Result>
{
    public async Task<Result> Handle(RemoveCierreCajaDetalleCommand request, CancellationToken ct)
    {
        var detalle = await db.CierresCajaDetalle
            .FirstOrDefaultAsync(d => d.CierreId == request.CierreCajaId && d.Id == request.CierreCajaDetalleId, ct);

        if (detalle is null)
            return Result.Failure("Linea de detalle no encontrada.");

        db.CierresCajaDetalle.Remove(detalle);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class CreateCierreCajaCommandValidator : AbstractValidator<CreateCierreCajaCommand>
{
    public CreateCierreCajaCommandValidator()
    {
        RuleFor(x => x.UsuarioId).GreaterThan(0);
        RuleFor(x => x.NroCierre).GreaterThan(0);
        RuleFor(x => x.FechaCierre).GreaterThanOrEqualTo(x => x.FechaApertura);
    }
}

public class RegistrarControlTesoreriaCierreCajaCommandValidator : AbstractValidator<RegistrarControlTesoreriaCierreCajaCommand>
{
    public RegistrarControlTesoreriaCierreCajaCommandValidator()
    {
        RuleFor(x => x.CierreCajaId).GreaterThan(0);
    }
}

public class AddCierreCajaDetalleCommandValidator : AbstractValidator<AddCierreCajaDetalleCommand>
{
    public AddCierreCajaDetalleCommandValidator()
    {
        RuleFor(x => x.CierreCajaId).GreaterThan(0);
        RuleFor(x => x.CajaCuentaBancariaId).GreaterThan(0);
    }
}

public class RemoveCierreCajaDetalleCommandValidator : AbstractValidator<RemoveCierreCajaDetalleCommand>
{
    public RemoveCierreCajaDetalleCommandValidator()
    {
        RuleFor(x => x.CierreCajaId).GreaterThan(0);
        RuleFor(x => x.CierreCajaDetalleId).GreaterThan(0);
    }
}
