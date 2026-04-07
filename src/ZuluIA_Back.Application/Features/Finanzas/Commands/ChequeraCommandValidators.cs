using FluentValidation;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreateChequeraCommandValidator : AbstractValidator<CreateChequeraCommand>
{
    public CreateChequeraCommandValidator()
    {
        RuleFor(x => x.CajaId).GreaterThan(0);
        RuleFor(x => x.Banco).NotEmpty();
        RuleFor(x => x.NroCuenta).NotEmpty();
        RuleFor(x => x.NroDesde).GreaterThan(0);
        RuleFor(x => x.NroHasta).GreaterThan(0);
        RuleFor(x => x).Must(x => x.NroHasta >= x.NroDesde)
            .WithMessage("El número hasta debe ser mayor o igual al número desde.");
    }
}

public class UpdateChequeraCommandValidator : AbstractValidator<UpdateChequeraCommand>
{
    public UpdateChequeraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Banco).NotEmpty();
        RuleFor(x => x.NroCuenta).NotEmpty();
    }
}

public class UsarChequeCommandValidator : AbstractValidator<UsarChequeCommand>
{
    public UsarChequeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Numero).GreaterThan(0);
    }
}

public class ActivateChequeraCommandValidator : AbstractValidator<ActivateChequeraCommand>
{
    public ActivateChequeraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateChequeraCommandValidator : AbstractValidator<DeactivateChequeraCommand>
{
    public DeactivateChequeraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}