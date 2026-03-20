using EvalSystem.Domain.Common;
using EvalSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EvalSystem.Infrastructure.Persistence;

public class EvalSystemDbContext : DbContext
{
    public EvalSystemDbContext(DbContextOptions<EvalSystemDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Tecnologia> Tecnologias => Set<Tecnologia>();
    public DbSet<Evaluacion> Evaluaciones => Set<Evaluacion>();
    public DbSet<EvaluacionSeccion> EvaluacionSecciones => Set<EvaluacionSeccion>();
    public DbSet<Pregunta> Preguntas => Set<Pregunta>();
    public DbSet<OpcionRespuesta> OpcionesRespuesta => Set<OpcionRespuesta>();
    public DbSet<ProcesoSeleccion> ProcesosSeleccion => Set<ProcesoSeleccion>();
    public DbSet<ProcesoCandidato> ProcesoCandidatos => Set<ProcesoCandidato>();
    public DbSet<ProcesoEvaluacion> ProcesoEvaluaciones => Set<ProcesoEvaluacion>();
    public DbSet<SesionEvaluacion> SesionesEvaluacion => Set<SesionEvaluacion>();
    public DbSet<RespuestaCandidato> RespuestasCandidato => Set<RespuestaCandidato>();
    public DbSet<ResultadoEvaluacion> ResultadosEvaluacion => Set<ResultadoEvaluacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft delete filter global
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(EvalSystemDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        // Configuraciones de entidades
        ConfigureUsuario(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureTecnologia(modelBuilder);
        ConfigureEvaluacion(modelBuilder);
        ConfigureEvaluacionSeccion(modelBuilder);
        ConfigurePregunta(modelBuilder);
        ConfigureOpcionRespuesta(modelBuilder);
        ConfigureProcesoSeleccion(modelBuilder);
        ConfigureProcesoCandidato(modelBuilder);
        ConfigureProcesoEvaluacion(modelBuilder);
        ConfigureSesionEvaluacion(modelBuilder);
        ConfigureRespuestaCandidato(modelBuilder);
        ConfigureResultadoEvaluacion(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    // ── Configuraciones ──────────────────────────────────────────

    private static void ConfigureUsuario(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Nombre).HasMaxLength(200).IsRequired();
            e.Property(u => u.Email).HasMaxLength(300).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.Rol).HasConversion<int>();
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder mb)
    {
        mb.Entity<RefreshToken>(e =>
        {
            e.HasIndex(r => r.Token).IsUnique();
            e.Property(r => r.Token).HasMaxLength(500).IsRequired();
            e.HasOne(r => r.Usuario).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.UsuarioId);
        });
    }

    private static void ConfigureTecnologia(ModelBuilder mb)
    {
        mb.Entity<Tecnologia>(e =>
        {
            e.HasIndex(t => t.Nombre).IsUnique();
            e.Property(t => t.Nombre).HasMaxLength(100).IsRequired();
            e.Property(t => t.Descripcion).HasMaxLength(500);
        });
    }

    private static void ConfigureEvaluacion(ModelBuilder mb)
    {
        mb.Entity<Evaluacion>(e =>
        {
            e.Property(ev => ev.Nombre).HasMaxLength(300).IsRequired();
            e.Property(ev => ev.Descripcion).HasMaxLength(1000);
            e.Property(ev => ev.Nivel).HasConversion<int>();
            e.HasOne(ev => ev.Tecnologia).WithMany(t => t.Evaluaciones).HasForeignKey(ev => ev.TecnologiaId);
        });
    }

    private static void ConfigureEvaluacionSeccion(ModelBuilder mb)
    {
        mb.Entity<EvaluacionSeccion>(e =>
        {
            e.Property(s => s.Nombre).HasMaxLength(200).IsRequired();
            e.Property(s => s.Descripcion).HasMaxLength(500);
            e.HasOne(s => s.Evaluacion).WithMany(ev => ev.Secciones).HasForeignKey(s => s.EvaluacionId);
        });
    }

    private static void ConfigurePregunta(ModelBuilder mb)
    {
        mb.Entity<Pregunta>(e =>
        {
            e.Property(p => p.Texto).HasMaxLength(2000).IsRequired();
            e.Property(p => p.Tipo).HasConversion<int>();
            e.Property(p => p.Explicacion).HasMaxLength(2000);
            e.HasOne(p => p.Seccion).WithMany(s => s.Preguntas).HasForeignKey(p => p.SeccionId);
        });
    }

    private static void ConfigureOpcionRespuesta(ModelBuilder mb)
    {
        mb.Entity<OpcionRespuesta>(e =>
        {
            e.Property(o => o.Texto).HasMaxLength(1000).IsRequired();
            e.HasOne(o => o.Pregunta).WithMany(p => p.Opciones).HasForeignKey(o => o.PreguntaId);
        });
    }

    private static void ConfigureProcesoSeleccion(ModelBuilder mb)
    {
        mb.Entity<ProcesoSeleccion>(e =>
        {
            e.Property(p => p.Nombre).HasMaxLength(300).IsRequired();
            e.Property(p => p.Descripcion).HasMaxLength(1000);
            e.Property(p => p.Puesto).HasMaxLength(200);
            e.Property(p => p.Estado).HasConversion<int>();
            e.HasOne(p => p.Creador).WithMany().HasForeignKey(p => p.CreadorId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProcesoCandidato(ModelBuilder mb)
    {
        mb.Entity<ProcesoCandidato>(e =>
        {
            e.HasIndex(pc => new { pc.ProcesoId, pc.CandidatoId }).IsUnique();
            e.HasOne(pc => pc.Proceso).WithMany(p => p.Candidatos).HasForeignKey(pc => pc.ProcesoId);
            e.HasOne(pc => pc.Candidato).WithMany().HasForeignKey(pc => pc.CandidatoId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProcesoEvaluacion(ModelBuilder mb)
    {
        mb.Entity<ProcesoEvaluacion>(e =>
        {
            e.HasIndex(pe => new { pe.ProcesoId, pe.EvaluacionId }).IsUnique();
            e.HasOne(pe => pe.Proceso).WithMany(p => p.Evaluaciones).HasForeignKey(pe => pe.ProcesoId);
            e.HasOne(pe => pe.Evaluacion).WithMany(ev => ev.ProcesoEvaluaciones).HasForeignKey(pe => pe.EvaluacionId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSesionEvaluacion(ModelBuilder mb)
    {
        mb.Entity<SesionEvaluacion>(e =>
        {
            e.Property(s => s.Estado).HasConversion<int>();
            e.Property(s => s.ScoreObtenido).IsRequired(false);
            e.HasOne(s => s.Candidato).WithMany(u => u.Sesiones).HasForeignKey(s => s.CandidatoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Evaluacion).WithMany(ev => ev.Sesiones).HasForeignKey(s => s.EvaluacionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Proceso).WithMany().HasForeignKey(s => s.ProcesoId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });
    }

    private static void ConfigureRespuestaCandidato(ModelBuilder mb)
    {
        mb.Entity<RespuestaCandidato>(e =>
        {
            e.Property(r => r.Respuesta).HasMaxLength(5000).IsRequired();
            e.Property(r => r.ComentarioRevisor).HasMaxLength(2000);
            e.HasIndex(r => new { r.SesionId, r.PreguntaId }).IsUnique();
            e.HasOne(r => r.Sesion).WithMany(s => s.Respuestas).HasForeignKey(r => r.SesionId);
            e.HasOne(r => r.Pregunta).WithMany(p => p.Respuestas).HasForeignKey(r => r.PreguntaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.OpcionSeleccionada).WithMany().HasForeignKey(r => r.OpcionSeleccionadaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });
    }

    private static void ConfigureResultadoEvaluacion(ModelBuilder mb)
    {
        mb.Entity<ResultadoEvaluacion>(e =>
        {
            e.HasIndex(r => r.SesionId).IsUnique();
            e.Property(r => r.ScoreTotal).HasColumnType("decimal(5,2)");
            e.Property(r => r.ScorePorSeccion).HasMaxLength(4000);
            e.Property(r => r.BrechasIdentificadas).HasMaxLength(4000);
            e.Property(r => r.RecomendacionIA).HasMaxLength(8000);
            e.Property(r => r.FortalezasIdentificadas).HasMaxLength(4000);
            e.HasOne(r => r.Sesion).WithOne(s => s.Resultado).HasForeignKey<ResultadoEvaluacion>(r => r.SesionId);
        });
    }
}
