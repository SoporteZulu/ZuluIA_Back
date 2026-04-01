using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

internal static class TerceroCuentaCorrienteReadModelLoader
{
    public static TerceroCuentaCorrienteDto Load(
        Tercero tercero,
        TerceroPerfilComercial? perfil)
        => new()
        {
            LimiteSaldo = perfil?.SaldoMaximoVigente,
            VigenciaLimiteSaldoDesde = perfil?.VigenciaSaldoDesde,
            VigenciaLimiteSaldoHasta = perfil?.VigenciaSaldoHasta,
            LimiteCreditoTotal = tercero.LimiteCredito,
            VigenciaLimiteCreditoTotalDesde = tercero.VigenciaCreditoDesde,
            VigenciaLimiteCreditoTotalHasta = tercero.VigenciaCreditoHasta
        };
}
