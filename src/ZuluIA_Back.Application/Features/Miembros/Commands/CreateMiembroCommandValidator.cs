using FluentValidation;

namespace ZuluIA_Back.Application.Features.Miembros.Commands;

public class CreateMiembroCommandValidator : AbstractValidator<CreateMiembroCommand>
{
    public CreateMiembroCommandValidator()
    {
        RuleFor(x => x.Legajo)
            .NotEmpty();

        RuleFor(x => x.Nombre)
            .NotEmpty();

        RuleFor(x => x.NroDocumento)
            .NotEmpty();

        RuleFor(x => x.TipoDocumentoId)
            .GreaterThan(0);

        RuleFor(x => x.CondicionIvaId)
            .GreaterThan(0);
    }
}