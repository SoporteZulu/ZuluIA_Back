using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreateChequeraCommandHandler(
    IRepository<Chequera> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateChequeraCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateChequeraCommand request, CancellationToken ct)
    {
        Chequera chequera;

        try
        {
            chequera = Chequera.Crear(
                request.CajaId,
                request.Banco,
                request.NroCuenta,
                request.NroDesde,
                request.NroHasta,
                request.Observacion,
                currentUser.UserId);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(chequera, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(chequera.Id);
    }
}