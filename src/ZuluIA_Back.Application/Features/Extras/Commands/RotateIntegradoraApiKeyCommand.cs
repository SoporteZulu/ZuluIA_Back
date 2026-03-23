using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record RotateIntegradoraApiKeyCommand(long Id, string NuevaApiKey) : IRequest<Result>;