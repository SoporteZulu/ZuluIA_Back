using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class RegionDiagnosticaConfiguration : IEntityTypeConfiguration<RegionDiagnostica>
{
    public void Configure(EntityTypeBuilder<RegionDiagnostica> builder)
    {
        builder.ToTable("regiones_diagnosticas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class AspectoDiagnosticoConfiguration : IEntityTypeConfiguration<AspectoDiagnostico>
{
    public void Configure(EntityTypeBuilder<AspectoDiagnostico> builder)
    {
        builder.ToTable("aspectos_diagnostico");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.RegionId).HasColumnName("region_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Peso).HasColumnName("peso").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.RegionId, x.Codigo }).IsUnique();
    }
}

public class VariableDiagnosticaConfiguration : IEntityTypeConfiguration<VariableDiagnostica>
{
    public void Configure(EntityTypeBuilder<VariableDiagnostica> builder)
    {
        builder.ToTable("variables_diagnosticas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.AspectoId).HasColumnName("aspecto_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoVariableDiagnostica>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Requerida).HasColumnName("requerida").IsRequired();
        builder.Property(x => x.Peso).HasColumnName("peso").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasMany(x => x.Opciones).WithOne().HasForeignKey(x => x.VariableId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.AspectoId, x.Codigo }).IsUnique();
    }
}

public class VariableDiagnosticaOpcionConfiguration : IEntityTypeConfiguration<VariableDiagnosticaOpcion>
{
    public void Configure(EntityTypeBuilder<VariableDiagnosticaOpcion> builder)
    {
        builder.ToTable("variables_diagnosticas_opciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.VariableId).HasColumnName("variable_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ValorNumerico).HasColumnName("valor_numerico").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Orden).HasColumnName("orden").IsRequired();
        builder.HasIndex(x => new { x.VariableId, x.Codigo }).IsUnique();
    }
}

public class PlantillaDiagnosticaConfiguration : IEntityTypeConfiguration<PlantillaDiagnostica>
{
    public void Configure(EntityTypeBuilder<PlantillaDiagnostica> builder)
    {
        builder.ToTable("plantillas_diagnosticas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasMany(x => x.Variables).WithOne().HasForeignKey(x => x.PlantillaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class PlantillaDiagnosticaVariableConfiguration : IEntityTypeConfiguration<PlantillaDiagnosticaVariable>
{
    public void Configure(EntityTypeBuilder<PlantillaDiagnosticaVariable> builder)
    {
        builder.ToTable("plantillas_diagnosticas_variables");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.PlantillaId).HasColumnName("plantilla_id").IsRequired();
        builder.Property(x => x.VariableId).HasColumnName("variable_id").IsRequired();
        builder.Property(x => x.Orden).HasColumnName("orden").IsRequired();
        builder.HasIndex(x => new { x.PlantillaId, x.VariableId }).IsUnique();
    }
}

public class PlanillaDiagnosticaConfiguration : IEntityTypeConfiguration<PlanillaDiagnostica>
{
    public void Configure(EntityTypeBuilder<PlanillaDiagnostica> builder)
    {
        builder.ToTable("planillas_diagnosticas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.PlantillaId).HasColumnName("plantilla_id").IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.ResultadoTotal).HasColumnName("resultado_total").HasPrecision(18, 4);
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoPlanillaDiagnostica>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasMany(x => x.Respuestas).WithOne().HasForeignKey(x => x.PlanillaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.PlantillaId);
        builder.HasIndex(x => x.Estado);
    }
}

public class PlanillaDiagnosticaRespuestaConfiguration : IEntityTypeConfiguration<PlanillaDiagnosticaRespuesta>
{
    public void Configure(EntityTypeBuilder<PlanillaDiagnosticaRespuesta> builder)
    {
        builder.ToTable("planillas_diagnosticas_respuestas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.PlanillaId).HasColumnName("planilla_id").IsRequired();
        builder.Property(x => x.VariableId).HasColumnName("variable_id").IsRequired();
        builder.Property(x => x.OpcionId).HasColumnName("opcion_id");
        builder.Property(x => x.ValorTexto).HasColumnName("valor_texto");
        builder.Property(x => x.ValorNumerico).HasColumnName("valor_numerico").HasPrecision(18, 4);
        builder.Property(x => x.PuntajeObtenido).HasColumnName("puntaje_obtenido").HasPrecision(18, 4).IsRequired();
        builder.HasIndex(x => new { x.PlanillaId, x.VariableId }).IsUnique();
    }
}
