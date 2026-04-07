using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Referencia;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MotivoDebitoConfiguration : IEntityTypeConfiguration<MotivoDebito>
{
    public void Configure(EntityTypeBuilder<MotivoDebito> builder)
    {
        builder.ToTable("motivos_debito");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.EsFiscal)
            .HasColumnName("es_fiscal")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.RequiereDocumentoOrigen)
            .HasColumnName("requiere_documento_origen")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.AfectaCuentaCorriente)
            .HasColumnName("afecta_cuenta_corriente")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.Activo)
            .HasColumnName("activo")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(m => m.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(m => m.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(m => m.UpdatedBy)
            .HasColumnName("updated_by");

        builder.HasIndex(m => m.Codigo)
            .IsUnique()
            .HasDatabaseName("ix_motivos_debito_codigo");

        builder.HasIndex(m => m.Activo)
            .HasDatabaseName("ix_motivos_debito_activo");
    }
}
