using OrbitaVerde.API.Domain.Enums;

namespace OrbitaVerde.API.Domain.Entities;

/// <summary>
/// Representa uma região solar ativa ou área terrestre em monitoramento.
/// Entidade central da plataforma OrbitaVerde.
/// </summary>
public class RegiaoAtiva
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public NivelAlerta NivelAtual { get; set; } = NivelAlerta.NORMAL;
    public DateTime PrimeiraDeteccao { get; set; } = DateTime.UtcNow;
    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
    public bool Ativa { get; set; } = true;

    // FK para satélite que detectou
    public int? SateliteId { get; set; }
    public Satelite? Satelite { get; set; }

    // Alertas gerados por esta região
    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();

    /// <summary>
    /// Calcula o tempo decorrido desde a primeira detecção.
    /// </summary>
    public TimeSpan TempoDesdeDeteccao()
        => DateTime.UtcNow - PrimeiraDeteccao;

    public override string ToString()
        => $"Região '{Nome}' | Nível: {NivelAtual} | " +
           $"Ativa há: {TempoDesdeDeteccao().TotalHours:F1}h";
}
