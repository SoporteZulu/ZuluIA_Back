using MediatR;
using ZuluIA_Back.Application.Features.Cobros.DTOs;

namespace ZuluIA_Back.Application.Features.Cobros.Queries;

public record GetCobroByIdQuery(long Id) : IRequest<CobroDto?>;