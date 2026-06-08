namespace OrbitaVerde.API.Domain.Entities;

/// <summary>
/// Alerta específico para eventos de flare solar.
/// Herda de Alerta (herança concreta) — conecta com o módulo IAML da plataforma.
/// </summary>
public class AlertaFlare : Alerta
{
    /// <summary>Classe do flare: A, B, C, M ou X.</summary>
    public string ClasseFlare { get; set; } = "C";

    /// <summary>Intensidade em watts por metro quadrado (W/m²).</summary>
    public double IntensidadeWm2 { get; set; }

    /// <summary>Duração estimada do evento em minutos.</summary>
    public int DuracaoMinutos { get; set; }

    /// <summary>Probabilidade de impacto geomagnético (0–100%).</summary>
    public double ProbabilidadeImpacto { get; set; }

    public override string ObterCategoria() => "Flare Solar";

    public override string GerarMensagem()
    {
        var base_ = base.GerarMensagem();
        return $"{base_} | Classe: {ClasseFlare} | " +
               $"Intensidade: {IntensidadeWm2:E2} W/m² | " +
               $"Impacto geomagnético: {ProbabilidadeImpacto:F1}%";
    }
}
