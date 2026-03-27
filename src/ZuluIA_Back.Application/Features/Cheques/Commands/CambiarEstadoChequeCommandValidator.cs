using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class CambiarEstadoChequeCommandValidator : AbstractValidator<CambiarEstadoChequeCommand>
{
    public CambiarEstadoChequeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        When(x => x.Accion == AccionCheque.Depositar || x.Accion == AccionCheque.Acreditar,
            () => RuleFor(x => x.Fecha).NotNull().WithMessage("La fecha es obligatoria para la acción indicada."));
    }
}
