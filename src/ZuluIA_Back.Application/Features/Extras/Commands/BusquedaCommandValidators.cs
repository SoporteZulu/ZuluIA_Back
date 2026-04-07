using FluentValidation;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateBusquedaCommandValidator : AbstractValidator<CreateBusquedaCommand>
{
    public CreateBusquedaCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Modulo).NotEmpty();
        RuleFor(x => x.FiltrosJson).NotEmpty();
    }
}

public class UpdateBusquedaCommandValidator : AbstractValidator<UpdateBusquedaCommand>
{
    public UpdateBusquedaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.FiltrosJson).NotEmpty();
    }
}

public class DeleteBusquedaCommandValidator : AbstractValidator<DeleteBusquedaCommand>
{
    public DeleteBusquedaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
