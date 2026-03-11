using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class CreateLiquidacionSueldoCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<CreateLiquidacionSueldoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateLiquidacionSueldoCommand request,
        CancellationToken ct)
    {
        // Validar que no exista liquidación para ese período
        var existe = await db.LiquidacionesSueldo
            .AnyAsync(x =>
                x.EmpleadoId == request.EmpleadoId &&
                x.Anio       == request.Anio        &&
                x.Mes        == request.Mes, ct);

        if (existe)
            return Result.Failure<long>(
                $"Ya existe una liquidación para el período {request.Anio}/{request.Mes:D2}.");

        var liq = LiquidacionSueldo.Crear(
            request.EmpleadoId,
            request.SucursalId,
            request.Anio,
            request.Mes,
            request.SueldoBasico,
            request.TotalHaberes,
            request.TotalDescuentos,
            request.MonedaId,
            request.Observacion);

        await db.LiquidacionesSueldo.AddAsync(liq, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(liq.Id);
    }
}