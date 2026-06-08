using Microsoft.EntityFrameworkCore;
using OrbitaVerde.API.Domain.Entities;
using OrbitaVerde.API.Domain.Enums;

namespace OrbitaVerde.API.Data;

/// <summary>
/// Contexto do Entity Framework Core para a plataforma OrbitaVerde.
/// Gerencia o acesso ao banco de dados SQLite.
/// </summary>
public class OrbitaVerdeContext : DbContext
{
    public OrbitaVerdeContext(DbContextOptions<OrbitaVerdeContext> options) : base(options) { }

    public DbSet<Satelite> Satelites => Set<Satelite>();
    public DbSet<SensorSolo> SensoresSolo => Set<SensorSolo>();
    public DbSet<RegiaoAtiva> RegioesAtivas => Set<RegiaoAtiva>();
    public DbSet<AlertaFlare> AlertasFlare => Set<AlertaFlare>();
    public DbSet<AlertaQueimada> AlertasQueimada => Set<AlertaQueimada>();
    public DbSet<Alerta> Alertas => Set<Alerta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Herança de EquipamentoEspacial — TPH (Table Per Hierarchy)
        modelBuilder.Entity<EquipamentoEspacial>()
            .HasDiscriminator<string>("TipoEquipamento")
            .HasValue<Satelite>("Satelite")
            .HasValue<SensorSolo>("SensorSolo");

        // Herança de Alerta — TPH
        modelBuilder.Entity<Alerta>()
            .HasDiscriminator<string>("TipoAlerta")
            .HasValue<AlertaFlare>("AlertaFlare")
            .HasValue<AlertaQueimada>("AlertaQueimada");

        // Satélite → RegiaoAtiva (1:N)
        modelBuilder.Entity<RegiaoAtiva>()
            .HasOne(r => r.Satelite)
            .WithMany(s => s.RegioesMonitoradas)
            .HasForeignKey(r => r.SateliteId)
            .OnDelete(DeleteBehavior.SetNull);

        // RegiaoAtiva → Alerta (1:N)
        modelBuilder.Entity<Alerta>()
            .HasOne(a => a.RegiaoAtiva)
            .WithMany(r => r.Alertas)
            .HasForeignKey(a => a.RegiaoAtivaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enum → string no SQLite
        modelBuilder.Entity<RegiaoAtiva>()
            .Property(r => r.NivelAtual)
            .HasConversion<string>();

        modelBuilder.Entity<Alerta>()
            .Property(a => a.Nivel)
            .HasConversion<string>();

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Satelite>().HasData(
            new Satelite
            {
                Id = 1,
                Nome = "GOES-16",
                Fabricante = "NASA / NOAA",
                DataLancamento = new DateTime(2016, 11, 19, 0, 0, 0, DateTimeKind.Utc),
                DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EstaAtivo = true,
                AltitudeOrbitaKm = 35786,
                TipoOrbita = "GEO",
                CoberturaDegraus = 120,
                QuantidadeSensores = 6
            },
            new Satelite
            {
                Id = 2,
                Nome = "Aqua (MODIS)",
                Fabricante = "NASA",
                DataLancamento = new DateTime(2002, 5, 4, 0, 0, 0, DateTimeKind.Utc),
                DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EstaAtivo = true,
                AltitudeOrbitaKm = 705,
                TipoOrbita = "LEO",
                CoberturaDegraus = 360,
                QuantidadeSensores = 36
            }
        );

        modelBuilder.Entity<SensorSolo>().HasData(
            new SensorSolo
            {
                Id = 3,
                Nome = "Sensor-AM-001",
                Fabricante = "INPE",
                DataLancamento = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EstaAtivo = true,
                Localizacao = "Manaus, AM",
                Latitude = -3.1190,
                Longitude = -60.0217,
                TipoSensor = "TERMICO",
                UltimaLeitura = 302.5,
                UltimaLeituraEm = new DateTime(2026, 6, 7, 20, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<RegiaoAtiva>().HasData(
            new RegiaoAtiva
            {
                Id = 1,
                Nome = "Região Solar AR3800",
                Descricao = "Região ativa no hemisfério norte solar com alta atividade magnética",
                Latitude = 25.0,
                Longitude = 130.0,
                NivelAtual = NivelAlerta.ALERTA,
                PrimeiraDeteccao = new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc),
                UltimaAtualizacao = new DateTime(2026, 6, 7, 18, 0, 0, DateTimeKind.Utc),
                Ativa = true,
                SateliteId = 1
            },
            new RegiaoAtiva
            {
                Id = 2,
                Nome = "Foco Pará-Sul",
                Descricao = "Foco de queimada detectado no sul do Pará, bioma Amazônia",
                Latitude = -6.5,
                Longitude = -52.3,
                NivelAtual = NivelAlerta.PERIGO,
                PrimeiraDeteccao = new DateTime(2026, 6, 6, 8, 0, 0, DateTimeKind.Utc),
                UltimaAtualizacao = new DateTime(2026, 6, 7, 16, 0, 0, DateTimeKind.Utc),
                Ativa = true,
                SateliteId = 2
            }
        );

        modelBuilder.Entity<AlertaFlare>().HasData(
            new AlertaFlare
            {
                Id = 1,
                Titulo = "Flare Classe M detectado em AR3800",
                Descricao = "Evento de flare solar de classe M com potencial impacto em satélites de comunicação",
                Nivel = NivelAlerta.ALERTA,
                CriadoEm = new DateTime(2026, 6, 5, 14, 30, 0, DateTimeKind.Utc),
                RegiaoAtivaId = 1,
                ClasseFlare = "M",
                IntensidadeWm2 = 1e-5,
                DuracaoMinutos = 45,
                ProbabilidadeImpacto = 65.0
            }
        );

        modelBuilder.Entity<AlertaQueimada>().HasData(
            new AlertaQueimada
            {
                Id = 2,
                Titulo = "Queimada de grande porte no sul do Pará",
                Descricao = "Foco de incêndio com área estimada acima de 800 hectares detectado pelo MODIS",
                Nivel = NivelAlerta.PERIGO,
                CriadoEm = new DateTime(2026, 6, 6, 10, 0, 0, DateTimeKind.Utc),
                RegiaoAtivaId = 2,
                AreaHectares = 850.5,
                TemperaturaKelvin = 580.0,
                Bioma = "Amazônia",
                FonteDeteccao = "SATELLITE"
            }
        );
    }
}
