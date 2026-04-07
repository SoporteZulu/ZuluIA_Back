using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class EnviarRequisicionCompraCommandValidator : AbstractValidator<EnviarRequisicionCompraCommand>
{
    public EnviarRequisicionCompraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AprobarRequisicionCompraCommandValidator : AbstractValidator<AprobarRequisicionCompraCommand>
{
    public AprobarRequisicionCompraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class RechazarRequisicionCompraCommandValidator : AbstractValidator<RechazarRequisicionCompraCommand>
{
    public RechazarRequisicionCompraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CancelarRequisicionCompraCommandValidator : AbstractValidator<CancelarRequisicionCompraCommand>
{
    public CancelarRequisicionCompraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}