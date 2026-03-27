using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public record UpdateZonaCommand(long Id, string Descripcion) : IRequest<Result>;