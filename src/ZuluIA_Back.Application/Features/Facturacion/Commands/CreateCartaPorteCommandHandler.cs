using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CreateCartaPorteCommandHandler(
    ICartaPorteRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCartaPorteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateCartaPorteCommand request,
        CancellationToken ct)
    {
        var carta = CartaPorte.Crear(
            request.ComprobanteId,
            request.CuitRemitente,
            request.CuitDestinatario,
            request.CuitTransportista,
            request.FechaEmision,
            request.Observacion,
            currentUser.UserId);

        await repo.AddAsync(carta, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(carta.Id);
    }
}