using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record UpdateCentroCostoCommand(long Id, string Descripcion) : IRequest<Result>;