using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Extensions;
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
            .AsQueryableSafe()
            .AnySafeAsync(x =>
                x.EmpleadoId == request.EmpleadoId &&
                x.Anio       == request.Anio        &&
                x.Mes        == request.Mes, ct);

        if (existe)
            return Result.Failure<long>(
                $"Ya existe una liquidación para el período {request.Anio}/{request.Mes:D2}.");

        var empleado = await db.Empleados
            .AsNoTrackingSafe()
            .FirstOrDefaultSafeAsync(x => x.Id == request.EmpleadoId, ct);
        if (empleado is null)
            return Result.Failure<long>($"No se encontró el empleado ID {request.EmpleadoId}.");
        if (empleado.Estado != Domain.Enums.EstadoEmpleado.Activo)
            return Result.Failure<long>("Solo se pueden liquidar empleados activos.");
        if (!await db.Monedas.AsNoTrackingSafe().AnySafeAsync(x => x.Id == request.MonedaId, ct))
            return Result.Failure<long>($"No se encontró la moneda ID {request.MonedaId}.");

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