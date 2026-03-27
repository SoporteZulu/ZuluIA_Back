using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.BI;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CuboCampoConfiguration : IEntityTypeConfiguration<CuboCampo>
{
    public void Configure(EntityTypeBuilder<CuboCampo> builder)
    {
        builder.ToTable("BI_CUBOCAMPO");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ccam_id").UseIdentityColumn();

        builder.Property(x => x.CuboId)
               .HasColumnName("cub_id").IsRequired();

        builder.Property(x => x.SourceName)
               .HasColumnName("ccam_SourceName").HasMaxLength(200).IsRequired();

        builder.Property(x => x.Ubicacion)
               .HasColumnName("ccam_Location");

        builder.Property(x => x.Posicion)
               .HasColumnName("ccam_Position");

        builder.Property(x => x.TipoOrden)
               .HasColumnName("ccam_SortType");

        builder.Property(x => x.RankOn)
               .HasColumnName("ccam_RankOn");

        builder.Property(x => x.RankStyle)
               .HasColumnName("ccam_RankStyle");

        builder.Property(x => x.PiePaginaVisible)
               .HasColumnName("ccam_GroupFooterVisible").IsRequired();

        builder.Property(x => x.PiePaginaTipo)
               .HasColumnName("ccam_GroupFooterType");

        builder.Property(x => x.PiePaginaCaption)
               .HasColumnName("ccam_GroupFooterCaption").HasMaxLength(200);

        builder.Property(x => x.FuncionAgregado)
               .HasColumnName("ccam_AggregateFunc");

        builder.Property(x => x.Descripcion)
               .HasColumnName("ccam_descripcion").HasMaxLength(300);

        builder.Property(x => x.Observacion)
               .HasColumnName("ccam_observacion").HasMaxLength(500);

        builder.Property(x => x.Calculado)
               .HasColumnName("ccam_Calculated").IsRequired();

        builder.Property(x => x.Visible)
               .HasColumnName("ccam_Visible").IsRequired();

        builder.Property(x => x.VarName)
               .HasColumnName("ccam_VarName").HasMaxLength(200);

        builder.Property(x => x.Filtro)
               .HasColumnName("ccam_Filtro").HasMaxLength(2000);

        builder.Property(x => x.CampoPadreId)
               .HasColumnName("ccam_idPadre");

        builder.Property(x => x.Orden)
               .HasColumnName("ccam_Orden").IsRequired();

        builder.HasIndex(x => x.CuboId);
    }
}
