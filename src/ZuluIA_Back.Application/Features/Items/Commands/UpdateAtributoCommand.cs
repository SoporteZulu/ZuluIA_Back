using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record UpdateAtributoCommand(long Id, string Descripcion, string Tipo, bool Requerido)
    : IRequest<Result>;