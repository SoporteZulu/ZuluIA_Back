using MediatR;
using ZuluIA_Back.Application.Features.Items.DTOs;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public record GetItemByIdQuery(long Id) : IRequest<ItemDto?>;