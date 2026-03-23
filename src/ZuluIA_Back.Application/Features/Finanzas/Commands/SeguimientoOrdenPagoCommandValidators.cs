using FluentValidation;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreateSeguimientoOrdenPagoCommandValidator : AbstractValidator<CreateSeguimientoOrdenPagoCommand>
{
    public CreateSeguimientoOrdenPagoCommandValidator()
    {
        RuleFor(x => x.PagoId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Estado).NotEmpty();
    }
}

public class UpdateSeguimientoOrdenPagoObservacionCommandValidator : AbstractValidator<UpdateSeguimientoOrdenPagoObservacionCommand>
{
    public UpdateSeguimientoOrdenPagoObservacionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
