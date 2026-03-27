using FluentValidation;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ImportarOperacionesCommandValidator : AbstractValidator<ImportarOperacionesCommand>
{
    public ImportarOperacionesCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Operaciones).NotEmpty();
        RuleForEach(x => x.Operaciones).ChildRules(op =>
        {
            op.RuleFor(x => x.Referencia).NotEmpty();
            op.RuleFor(x => x.Tipo).NotEmpty();
        });
    }
}
