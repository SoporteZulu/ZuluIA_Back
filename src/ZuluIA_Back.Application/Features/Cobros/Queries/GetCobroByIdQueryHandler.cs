using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Cobros.DTOs;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cobros.Queries;

public class GetCobroByIdQueryHandler(
    IRepository<Cobro> repo,
    IMapper mapper)
    : IRequestHandler<GetCobroByIdQuery, CobroDto?>
{
    public async Task<CobroDto?> Handle(GetCobroByIdQuery request, CancellationToken ct)
    {
        var cobro = await repo.GetByIdAsync(request.Id, ct);
        return cobro is null ? null : mapper.Map<CobroDto>(cobro);
    }
}