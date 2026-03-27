using FluentValidation;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class CreateImpuestoCommandValidator : AbstractValidator<CreateImpuestoCommand>
{
    public CreateImpuestoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateImpuestoCommandValidator : AbstractValidator<UpdateImpuestoCommand>
{
    public UpdateImpuestoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.Tipo).NotEmpty();
    }
}

public class ActivateImpuestoCommandValidator : AbstractValidator<ActivateImpuestoCommand>
{
    public ActivateImpuestoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateImpuestoCommandValidator : AbstractValidator<DeactivateImpuestoCommand>
{
    public DeactivateImpuestoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AssignImpuestoTerceroCommandValidator : AbstractValidator<AssignImpuestoTerceroCommand>
{
    public AssignImpuestoTerceroCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
    }
}

public class UnassignImpuestoTerceroCommandValidator : AbstractValidator<UnassignImpuestoTerceroCommand>
{
    public UnassignImpuestoTerceroCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
    }
}

public class AssignImpuestoItemCommandValidator : AbstractValidator<AssignImpuestoItemCommand>
{
    public AssignImpuestoItemCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
    }
}

public class UnassignImpuestoItemCommandValidator : AbstractValidator<UnassignImpuestoItemCommand>
{
    public UnassignImpuestoItemCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
    }
}

public class AssignImpuestoSucursalCommandValidator : AbstractValidator<AssignImpuestoSucursalCommand>
{
    public AssignImpuestoSucursalCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
    }
}

public class UpdateImpuestoSucursalCommandValidator : AbstractValidator<UpdateImpuestoSucursalCommand>
{
    public UpdateImpuestoSucursalCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.AsignacionId).GreaterThan(0);
    }
}

public class DeleteImpuestoSucursalCommandValidator : AbstractValidator<DeleteImpuestoSucursalCommand>
{
    public DeleteImpuestoSucursalCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.AsignacionId).GreaterThan(0);
    }
}

public class AssignImpuestoTipoComprobanteCommandValidator : AbstractValidator<AssignImpuestoTipoComprobanteCommand>
{
    public AssignImpuestoTipoComprobanteCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
    }
}

public class DeleteImpuestoTipoComprobanteCommandValidator : AbstractValidator<DeleteImpuestoTipoComprobanteCommand>
{
    public DeleteImpuestoTipoComprobanteCommandValidator()
    {
        RuleFor(x => x.ImpuestoId).GreaterThan(0);
        RuleFor(x => x.AsignacionId).GreaterThan(0);
    }
}