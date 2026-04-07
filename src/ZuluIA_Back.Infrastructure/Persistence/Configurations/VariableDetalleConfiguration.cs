using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class VariableDetalleConfiguration : IEntityTypeConfiguration<VariableDetalle>
{
    public void Configure(EntityTypeBuilder<VariableDetalle> builder)
    {
        builder.ToTable("FRA_VARIABLESDETALLE");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("vard_id").UseIdentityColumn();

        builder.Property(x => x.VariableId)
               .HasColumnName("var_id").IsRequired();

        builder.Property(x => x.OpcionVariableId)
               .HasColumnName("opc_id");

        builder.Property(x => x.AplicaPuntajePenalizacion)
               .HasColumnName("vard_aplicaPuntajePenalizacion").IsRequired();

        builder.Property(x => x.VisualizarOpcion)
               .HasColumnName("vard_visualizarOpcion").IsRequired();

        builder.Property(x => x.PorcentajeIncidencia)
               .HasColumnName("vard_porcentajeIncidencia").HasPrecision(10, 4);

        builder.Property(x => x.ValorObjetivo)
               .HasColumnName("vard_valorObjetivo").HasPrecision(18, 4);

        builder.HasIndex(x => x.VariableId);
    }
}
