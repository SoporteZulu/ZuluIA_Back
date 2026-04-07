using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Franquicias;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class GrupoEconomicoConfiguration : IEntityTypeConfiguration<GrupoEconomico>
{
    public void Configure(EntityTypeBuilder<GrupoEconomico> builder)
    {
        builder.ToTable("grupos_economicos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class FranquiciaXRegionConfiguration : IEntityTypeConfiguration<FranquiciaXRegion>
{
    public void Configure(EntityTypeBuilder<FranquiciaXRegion> builder)
    {
        builder.ToTable("franquicias_x_regiones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.RegionId).HasColumnName("region_id").IsRequired();
        builder.Property(x => x.GrupoEconomicoId).HasColumnName("grupo_economico_id");
        builder.HasIndex(x => new { x.SucursalId, x.RegionId }).IsUnique();
    }
}

public class FranquiciaVariableXUsuarioConfiguration : IEntityTypeConfiguration<FranquiciaVariableXUsuario>
{
    public void Configure(EntityTypeBuilder<FranquiciaVariableXUsuario> builder)
    {
        builder.ToTable("franquicias_variables_x_usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.PlanTrabajoId).HasColumnName("plan_trabajo_id").IsRequired();
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(x => x.VariableId).HasColumnName("variable_id").IsRequired();
        builder.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.PlanTrabajoId, x.UsuarioId, x.VariableId }).IsUnique();
    }
}