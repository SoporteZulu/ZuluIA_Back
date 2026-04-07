using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class CambiarEstadoChequeCommandValidator : AbstractValidator<CambiarEstadoChequeCommand>
{
    public CambiarEstadoChequeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Accion)
            .IsInEnum().WithMessage("La acción indicada no es válida.");

        When(x => x.Accion == AccionCheque.Depositar || x.Accion == AccionCheque.Acreditar,
            () => RuleFor(x => x.Fecha)
                .NotNull()
                .WithMessage("La fecha es obligatoria para la acción indicada."));

        When(x => x.Accion == AccionCheque.Rechazar,
            () => RuleFor(x => x.ConceptoRechazo)
                .NotEmpty()
                .WithMessage("El concepto de rechazo es obligatorio."));

        When(x => x.Accion == AccionCheque.Entregar,
            () => RuleFor(x => x.TerceroId)
                .NotNull()
                .WithMessage("El tercero destinatario es obligatorio para entregar.")
                .GreaterThan(0)
                .WithMessage("El tercero destinatario es obligatorio para entregar."));

        When(x => x.Accion == AccionCheque.Endosar,
            () => RuleFor(x => x.TerceroId)
                .NotNull()
                .WithMessage("El tercero destino es obligatorio para endosar.")
                .GreaterThan(0)
                .WithMessage("El tercero destino es obligatorio para endosar."));

        When(x => x.Accion == AccionCheque.Anular,
            () => RuleFor(x => x.Observacion)
                .NotEmpty()
                .WithMessage("El motivo de anulación es obligatorio."));
    }
}
