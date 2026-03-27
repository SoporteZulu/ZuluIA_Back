using MediatR;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class RegistrarComprobanteFiscalAlternativoCommandHandler(PuntoVentaFiscalService service, IUnitOfWork uow) : IRequestHandler<RegistrarComprobanteFiscalAlternativoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarComprobanteFiscalAlternativoCommand request, CancellationToken ct)
    {
        try
        {
            var id = await service.RegistrarConFiscalAlternativoAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
