using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class ReplaceRemitoAtributosCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow) : IRequestHandler<ReplaceRemitoAtributosCommand, Result<IReadOnlyList<ComprobanteAtributoDto>>>
{
    public async Task<Result<IReadOnlyList<ComprobanteAtributoDto>>> Handle(ReplaceRemitoAtributosCommand request, CancellationToken ct)
    {
        var comprobante = await db.Comprobantes
            .FirstOrDefaultAsync(x => x.Id == request.ComprobanteId && x.DeletedAt == null, ct);

        if (comprobante is null)
            return Result.Failure<IReadOnlyList<ComprobanteAtributoDto>>($"No se encontró el comprobante ID {request.ComprobanteId}.");

        var existentes = await db.ComprobantesAtributos
            .Where(x => x.ComprobanteId == request.ComprobanteId && x.DeletedAt == null)
            .ToListAsync(ct);

        if (existentes.Count > 0)
            db.ComprobantesAtributos.RemoveRange(existentes);

        var nuevasEntidades = new List<ComprobanteAtributo>(request.Atributos.Count);
        foreach (var atributo in request.Atributos)
        {
            try
            {
                nuevasEntidades.Add(ComprobanteAtributo.Crear(
                    request.ComprobanteId,
                    atributo.Clave,
                    atributo.Valor,
                    atributo.TipoDato,
                    currentUser.UserId));
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<IReadOnlyList<ComprobanteAtributoDto>>(ex.Message);
            }
        }

        if (nuevasEntidades.Count > 0)
            await db.ComprobantesAtributos.AddRangeAsync(nuevasEntidades, ct);

        await uow.SaveChangesAsync(ct);

        IReadOnlyList<ComprobanteAtributoDto> dto = nuevasEntidades
            .OrderBy(x => x.Clave)
            .Select(x => new ComprobanteAtributoDto
            {
                Id = x.Id,
                Clave = x.Clave,
                Valor = x.Valor,
                TipoDato = x.TipoDato
            })
            .ToList()
            .AsReadOnly();

        return Result.Success(dto);
    }
}
