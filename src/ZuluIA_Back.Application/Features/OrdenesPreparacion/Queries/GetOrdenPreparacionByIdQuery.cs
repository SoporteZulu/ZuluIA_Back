using MediatR;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Queries;

public record GetOrdenPreparacionByIdQuery(long Id) : IRequest<Result<OrdenPreparacionDto>>;
