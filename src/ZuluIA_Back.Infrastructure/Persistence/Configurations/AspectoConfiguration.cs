using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AspectoConfiguration : IEntityTypeConfiguration<Aspecto>
{
    public void Configure(EntityTypeBuilder<Aspecto> builder)
    {
        builder.ToTable("aspecto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo").HasMaxLength(50).IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.CodigoEstructura)
               .HasColumnName("codigo_estructura").HasMaxLength(200);

        builder.Property(x => x.Orden)
               .HasColumnName("orden").IsRequired();

        builder.Property(x => x.Nivel)
               .HasColumnName("nivel").IsRequired();

        builder.Property(x => x.AspectoPadreId)
               .HasColumnName("aspecto_padre_id");

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion").HasMaxLength(500);

        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class VariableConfiguration : IEntityTypeConfiguration<Variable>
{
    public void Configure(EntityTypeBuilder<Variable> builder)
    {
        builder.ToTable("variable");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TipoVariableId)
               .HasColumnName("tipo_variable_id");

        builder.Property(x => x.TipoComprobanteId)
               .HasColumnName("tipo_comprobante_id");

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo").HasMaxLength(50).IsRequired();

        builder.Property(x => x.CodigoEstructura)
               .HasColumnName("codigo_estructura").HasMaxLength(200);

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion").HasMaxLength(500);

        builder.Property(x => x.AspectoId)
               .HasColumnName("aspecto_id");

        builder.Property(x => x.Nivel)
               .HasColumnName("nivel").IsRequired();

        builder.Property(x => x.Orden)
               .HasColumnName("orden").IsRequired();

        builder.Property(x => x.Condicionante)
               .HasColumnName("condicionante").HasMaxLength(500);

        builder.Property(x => x.Editable)
               .HasColumnName("editable").IsRequired();
    }
}
