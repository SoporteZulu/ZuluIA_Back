using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class UpsertRemitoCotCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow) : IRequestHandler<UpsertRemitoCotCommand, Result<ComprobanteCotDto>>
{
    public async Task<Result<ComprobanteCotDto>> Handle(UpsertRemitoCotCommand request, CancellationToken ct)
    {
        var comprobante = await db.Comprobantes
            .FirstOrDefaultAsync(x => x.Id == request.ComprobanteId && x.DeletedAt == null, ct);

        if (comprobante is null)
            return Result.Failure<ComprobanteCotDto>($"No se encontró el remito con ID {request.ComprobanteId}.");

        ComprobanteCot cot;
        try
        {
            cot = await db.ComprobantesCot
                .FirstOrDefaultAsync(x => x.ComprobanteId == request.ComprobanteId && x.DeletedAt == null, ct)
                ?? ComprobanteCot.Crear(
                    request.ComprobanteId,
                    request.Numero,
                    request.FechaVigencia,
                    comprobante.Fecha,
                    request.Descripcion,
                    currentUser.UserId);

            if (cot.Id == 0)
            {
                db.ComprobantesCot.Add(cot);
            }
            else
            {
                cot.Actualizar(request.Numero, request.FechaVigencia, comprobante.Fecha, request.Descripcion, currentUser.UserId);
            }
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<ComprobanteCotDto>(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<ComprobanteCotDto>(ex.Message);
        }

        await uow.SaveChangesAsync(ct);

        return Result.Success(new ComprobanteCotDto
        {
            Numero = cot.Numero,
            FechaVigencia = cot.FechaVigencia,
            Descripcion = cot.Descripcion
        });
    }
}
