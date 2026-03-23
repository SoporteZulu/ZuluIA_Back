using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class CambiarEstadoChequeCommandValidator : AbstractValidator<CambiarEstadoChequeCommand>
{
    public CambiarEstadoChequeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Accion)
            .IsInEnum();

        When(
            x => x.Accion is AccionCheque.Depositar or AccionCheque.Acreditar,
            () =>
            {
                RuleFor(x => x.Fecha)
                    .NotNull()
                    .WithMessage(x => x.Accion == AccionCheque.Depositar
                        ? "La fecha de deposito es obligatoria."
                        : "La fecha de acreditacion es obligatoria.");
            });
    }
}