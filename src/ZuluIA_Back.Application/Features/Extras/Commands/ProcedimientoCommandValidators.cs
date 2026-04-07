using FluentValidation;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateProcedimientoCommandValidator : AbstractValidator<CreateProcedimientoCommand>
{
    public CreateProcedimientoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.DefinicionJson).NotEmpty();
    }
}

public class UpdateProcedimientoCommandValidator : AbstractValidator<UpdateProcedimientoCommand>
{
    public UpdateProcedimientoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.DefinicionJson).NotEmpty();
    }
}

public class DeleteProcedimientoCommandValidator : AbstractValidator<DeleteProcedimientoCommand>
{
    public DeleteProcedimientoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
