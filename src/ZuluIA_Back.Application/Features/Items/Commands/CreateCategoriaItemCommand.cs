using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record CreateCategoriaItemCommand(
    long? ParentId,
    string Codigo,
    string Descripcion,
    short Nivel,
    string? OrdenNivel
) : IRequest<Result<long>>;