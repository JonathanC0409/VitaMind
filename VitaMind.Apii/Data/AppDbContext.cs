using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using VitaMind.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<BusquedaSemantica> BusquedaSemanticas { get; set; } = default!;
    public DbSet<ConsultaAgente> ConsultaAgentes { get; set; } = default!;
    public DbSet<ErrorIngreso> ErrorIngresos { get; set; } = default!;
    public DbSet<LogOperacion> LogOperaciones { get; set; } = default!;
    public DbSet<MetricaSistema> MetricaSistemas { get; set; } = default!;
    public DbSet<RegistroSalud> RegistrosSalud { get; set; } = default!;
    public DbSet<ResultadoBusqueda> ResultadoBusquedas { get; set; } = default!;
    public DbSet<ServicioEstado> ServicioEstados { get; set; } = default!;
    public DbSet<Usuario> Usuarios { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Extensión pgvector para Supabase/PostgreSQL
        modelBuilder.HasPostgresExtension("vector");

        // =========================
        // CLAVES PRIMARIAS
        // =========================
        modelBuilder.Entity<Usuario>()
            .HasKey(x => x.IdUsuario);

        modelBuilder.Entity<RegistroSalud>()
            .HasKey(x => x.IdRegistro);

        modelBuilder.Entity<ConsultaAgente>()
            .HasKey(x => x.IdConsulta);

        modelBuilder.Entity<BusquedaSemantica>()
            .HasKey(x => x.IdBusqueda);

        modelBuilder.Entity<ResultadoBusqueda>()
            .HasKey(x => x.IdResultado);

        modelBuilder.Entity<LogOperacion>()
            .HasKey(x => x.IdLog);

        modelBuilder.Entity<ErrorIngreso>()
            .HasKey(x => x.IdError);

        modelBuilder.Entity<MetricaSistema>()
            .HasKey(x => x.IdMetrica);

        modelBuilder.Entity<ServicioEstado>()
            .HasKey(x => x.IdServicio);

        // =========================
        // COLUMNAS VECTORIALES
        // =========================
        modelBuilder.Entity<RegistroSalud>()
            .Property(x => x.Embedding)
            .HasColumnType("vector(1536)");

        modelBuilder.Entity<BusquedaSemantica>()
            .Property(x => x.EmbeddingConsulta)
            .HasColumnType("vector(1536)");

        // =========================
        // RELACIONES
        // =========================

        // Usuario -> RegistrosSalud
        modelBuilder.Entity<RegistroSalud>()
            .HasOne(x => x.Usuario)
            .WithMany(x => x.RegistrosSalud)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        // RegistroSalud -> Registro similar
        modelBuilder.Entity<RegistroSalud>()
            .HasOne(x => x.RegistroSimilar)
            .WithMany()
            .HasForeignKey(x => x.IdRegistroSimilar)
            .OnDelete(DeleteBehavior.SetNull);

        // Usuario -> ConsultasAgente
        modelBuilder.Entity<ConsultaAgente>()
            .HasOne(x => x.Usuario)
            .WithMany(x => x.ConsultasAgente)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        // Usuario -> BusquedasSemanticas
        modelBuilder.Entity<BusquedaSemantica>()
            .HasOne(x => x.Usuario)
            .WithMany(x => x.BusquedasSemanticas)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        // Usuario -> LogsOperaciones
        modelBuilder.Entity<LogOperacion>()
            .HasOne(x => x.Usuario)
            .WithMany(x => x.LogsOperaciones)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.SetNull);

        // Usuario -> ErroresIngreso
        modelBuilder.Entity<ErrorIngreso>()
            .HasOne(x => x.Usuario)
            .WithMany(x => x.ErroresIngreso)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.SetNull);

        // BusquedaSemantica -> ResultadosBusqueda
        modelBuilder.Entity<ResultadoBusqueda>()
            .HasOne(x => x.BusquedaSemantica)
            .WithMany(x => x.ResultadosBusqueda)
            .HasForeignKey(x => x.IdBusqueda)
            .OnDelete(DeleteBehavior.Cascade);

        // RegistroSalud -> ResultadosBusqueda
        modelBuilder.Entity<ResultadoBusqueda>()
            .HasOne(x => x.RegistroSalud)
            .WithMany(x => x.ResultadosBusqueda)
            .HasForeignKey(x => x.IdRegistro)
            .OnDelete(DeleteBehavior.Cascade);
    }
}