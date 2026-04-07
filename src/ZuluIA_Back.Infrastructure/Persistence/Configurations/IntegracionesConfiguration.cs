using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ProcesoIntegracionJobConfiguration : IEntityTypeConfiguration<ProcesoIntegracionJob>
{
    public void Configure(EntityTypeBuilder<ProcesoIntegracionJob> builder)
    {
        builder.ToTable("procesos_integracion_jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoProcesoIntegracion>(v, true))
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ClaveIdempotencia).HasColumnName("clave_idempotencia").HasMaxLength(120);
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoProcesoIntegracion>(v, true))
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        builder.Property(x => x.TotalRegistros).HasColumnName("total_registros").IsRequired();
        builder.Property(x => x.RegistrosProcesados).HasColumnName("registros_procesados").IsRequired();
        builder.Property(x => x.RegistrosExitosos).HasColumnName("registros_exitosos").IsRequired();
        builder.Property(x => x.RegistrosConError).HasColumnName("registros_con_error").IsRequired();
        builder.Property(x => x.PayloadResumen).HasColumnName("payload_resumen");
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Tipo);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.FechaInicio);
        builder.HasIndex(x => new { x.Tipo, x.ClaveIdempotencia }).IsUnique();
    }
}

public class ProcesoIntegracionLogConfiguration : IEntityTypeConfiguration<ProcesoIntegracionLog>
{
    public void Configure(EntityTypeBuilder<ProcesoIntegracionLog> builder)
    {
        builder.ToTable("procesos_integracion_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.JobId).HasColumnName("job_id").IsRequired();
        builder.Property(x => x.Nivel)
            .HasColumnName("nivel")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<NivelLogIntegracion>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Mensaje).HasColumnName("mensaje").IsRequired();
        builder.Property(x => x.Referencia).HasColumnName("referencia").HasMaxLength(150);
        builder.Property(x => x.Payload).HasColumnName("payload");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.JobId);
        builder.HasIndex(x => x.Nivel);
    }
}

public class MonitorExportacionConfiguration : IEntityTypeConfiguration<MonitorExportacion>
{
    public void Configure(EntityTypeBuilder<MonitorExportacion> builder)
    {
        builder.ToTable("monitores_exportacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.UltimoJobId).HasColumnName("ultimo_job_id");
        builder.Property(x => x.UltimaEjecucion).HasColumnName("ultima_ejecucion");
        builder.Property(x => x.UltimoEstado)
            .HasColumnName("ultimo_estado")
            .HasConversion(v => v.HasValue ? v.Value.ToString().ToUpperInvariant() : null, v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<EstadoProcesoIntegracion>(v, true));
        builder.Property(x => x.RegistrosPendientes).HasColumnName("registros_pendientes").IsRequired();
        builder.Property(x => x.UltimoMensaje).HasColumnName("ultimo_mensaje");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class IntegracionExternaAuditConfiguration : IEntityTypeConfiguration<IntegracionExternaAudit>
{
    public void Configure(EntityTypeBuilder<IntegracionExternaAudit> builder)
    {
        builder.ToTable("integraciones_externas_audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Proveedor)
            .HasColumnName("proveedor")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<ProveedorIntegracionExterna>(v, true))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.Operacion).HasColumnName("operacion").HasMaxLength(80).IsRequired();
        builder.Property(x => x.ReferenciaTipo).HasColumnName("referencia_tipo").HasMaxLength(50);
        builder.Property(x => x.ReferenciaId).HasColumnName("referencia_id");
        builder.Property(x => x.Exitoso).HasColumnName("exitoso").IsRequired();
        builder.Property(x => x.Reintentos).HasColumnName("reintentos").IsRequired();
        builder.Property(x => x.TimeoutMs).HasColumnName("timeout_ms").IsRequired();
        builder.Property(x => x.CircuitBreakerAbierto).HasColumnName("circuit_breaker_abierto").IsRequired();
        builder.Property(x => x.DuracionMs).HasColumnName("duracion_ms").IsRequired();
        builder.Property(x => x.Ambiente).HasColumnName("ambiente").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Endpoint).HasColumnName("endpoint").HasMaxLength(300).IsRequired();
        builder.Property(x => x.CodigoError).HasColumnName("codigo_error").HasMaxLength(80);
        builder.Property(x => x.ErrorFuncional).HasColumnName("error_funcional").IsRequired();
        builder.Property(x => x.RequestPayload).HasColumnName("request_payload");
        builder.Property(x => x.ResponsePayload).HasColumnName("response_payload");
        builder.Property(x => x.MensajeError).HasColumnName("mensaje_error");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Proveedor);
        builder.HasIndex(x => x.Operacion);
        builder.HasIndex(x => x.ReferenciaId);
        builder.HasIndex(x => x.CreatedAt);
    }
}

public class BatchProgramacionConfiguration : IEntityTypeConfiguration<BatchProgramacion>
{
    public void Configure(EntityTypeBuilder<BatchProgramacion> builder)
    {
        builder.ToTable("batch_programaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.TipoProceso)
            .HasColumnName("tipo_proceso")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoProcesoIntegracion>(v, true))
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.IntervaloMinutos).HasColumnName("intervalo_minutos").IsRequired();
        builder.Property(x => x.ProximaEjecucion).HasColumnName("proxima_ejecucion").IsRequired();
        builder.Property(x => x.UltimaEjecucion).HasColumnName("ultima_ejecucion");
        builder.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.TipoProceso);
        builder.HasIndex(x => x.ProximaEjecucion);
        builder.HasIndex(x => x.Activa);
    }
}

public class ImpresionSpoolTrabajoConfiguration : IEntityTypeConfiguration<ImpresionSpoolTrabajo>
{
    public void Configure(EntityTypeBuilder<ImpresionSpoolTrabajo> builder)
    {
        builder.ToTable("impresion_spool_trabajos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.TipoTrabajo).HasColumnName("tipo_trabajo").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Destino).HasColumnName("destino").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoImpresionSpool>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Intentos).HasColumnName("intentos").IsRequired();
        builder.Property(x => x.ProximoIntento).HasColumnName("proximo_intento");
        builder.Property(x => x.PayloadGenerado).HasColumnName("payload_generado");
        builder.Property(x => x.MensajeError).HasColumnName("mensaje_error");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.ProximoIntento);
        builder.HasIndex(x => x.ComprobanteId);
    }
}
