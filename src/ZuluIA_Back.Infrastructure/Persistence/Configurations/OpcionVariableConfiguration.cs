using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OpcionVariableConfiguration : IEntityTypeConfiguration<OpcionVariable>
{
    public void Configure(EntityTypeBuilder<OpcionVariable> builder)
    {
        builder.ToTable("opcion_variable");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo").HasMaxLength(50).IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.Observaciones)
               .HasColumnName("observaciones").HasMaxLength(500);

        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}
