using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class ConfiguracionController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet("monedas")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonedas(CancellationToken ct)
    {
        var result = await db.Paises
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("condiciones-iva")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCondicionesIva()
    {
        var condiciones = new[]
        {
            new { Id = 0, Codigo = 0,  Descripcion = "Responsable Inscripto" },
            new { Id = 1, Codigo = 1,  Descripcion = "Monotributista" },
            new { Id = 2, Codigo = 2,  Descripcion = "Exento" },
            new { Id = 3, Codigo = 3,  Descripcion = "Consumidor Final" },
            new { Id = 4, Codigo = 4,  Descripcion = "Responsable No Inscripto" },
            new { Id = 5, Codigo = 5,  Descripcion = "No Responsable" }
        };

        return Ok(condiciones);
    }

    [HttpGet("tipos-documento")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTiposDocumento()
    {
        var tipos = new[]
        {
            new { Id = 1, Codigo = 80, Descripcion = "CUIT" },
            new { Id = 2, Codigo = 86, Descripcion = "CUIL" },
            new { Id = 3, Codigo = 96, Descripcion = "DNI"  },
            new { Id = 4, Codigo = 99, Descripcion = "Sin Identificar" }
        };

        return Ok(tipos);
    }
}