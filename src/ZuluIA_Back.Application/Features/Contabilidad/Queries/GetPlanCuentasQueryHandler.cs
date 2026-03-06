using MediatR;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public class GetPlanCuentasQueryHandler(IPlanCuentasRepository repo)
    : IRequestHandler<GetPlanCuentasQuery, IReadOnlyList<PlanCuentaDto>>
{
    public async Task<IReadOnlyList<PlanCuentaDto>> Handle(
        GetPlanCuentasQuery request,
        CancellationToken ct)
    {
        var cuentas = request.SoloImputables
            ? await repo.GetImputablesAsync(request.EjercicioId, ct)
            : await repo.GetByEjercicioAsync(request.EjercicioId, ct);

        // Construir árbol jerárquico
        var lookup = cuentas
            .Select(c => new PlanCuentaDto
            {
                Id            = c.Id,
                EjercicioId   = c.EjercicioId,
                IntegradoraId = c.IntegradoraId,
                CodigoCuenta  = c.CodigoCuenta,
                Denominacion  = c.Denominacion,
                Nivel         = c.Nivel,
                OrdenNivel    = c.OrdenNivel,
                Imputable     = c.Imputable,
                Tipo          = c.Tipo,
                SaldoNormal   = c.SaldoNormal
            })
            .ToDictionary(x => x.Id);

        var raices = new List<PlanCuentaDto>();

        foreach (var dto in lookup.Values)
        {
            if (dto.IntegradoraId.HasValue &&
                lookup.TryGetValue(dto.IntegradoraId.Value, out var padre))
                ((List<PlanCuentaDto>)padre.Subcuentas).Add(dto);
            else
                raices.Add(dto);
        }

        return raices
            .OrderBy(x => x.OrdenNivel)
            .ThenBy(x => x.CodigoCuenta)
            .ToList()
            .AsReadOnly();
    }
}