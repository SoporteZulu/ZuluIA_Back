using FluentValidation;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class ProcesarComprobanteDeuceCommandValidator : AbstractValidator<ProcesarComprobanteDeuceCommand>
{
    public ProcesarComprobanteDeuceCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.ReferenciaExterna).NotEmpty().MaximumLength(100);
    }
}
