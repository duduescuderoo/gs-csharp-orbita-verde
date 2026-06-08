using OrbitaVerde.API.Domain.Enums;

namespace OrbitaVerde.API.Domain.Entities;

/// <summary>
/// Representa um sensor físico instalado no solo para complementar o monitoramento satelital.
/// Herda de EquipamentoEspacial (segunda subclasse concreta — herança múltipla de tipos).
/// </summary>
public class SensorSolo : EquipamentoEspacial
{
    public string Localizacao { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TipoSensor { get; set; } = string.Empty; // TERMICO, FUMACA, UMIDADE, VISAO
    public double UltimaLeitura { get; set; }
    public DateTime UltimaLeituraEm { get; set; } = DateTime.UtcNow;

    public override string ObterTipo() => "Sensor de Solo";

    /// <summary>
    /// Verifica há quanto tempo a última leitura foi realizada.
    /// Se passou mais de 1 hora, entra em ALERTA (sem dados recentes).
    /// </summary>
    public override NivelAlerta ObterNivelAlerta()
    {
        if (!EstaAtivo) return NivelAlerta.PERIGO;

        var minutesSemLeitura = (DateTime.UtcNow - UltimaLeituraEm).TotalMinutes;
        return minutesSemLeitura switch
        {
            < 30 => NivelAlerta.NORMAL,
            < 60 => NivelAlerta.ALERTA,
            _ => NivelAlerta.PERIGO
        };
    }

    public override string RealizarMonitoramento()
    {
        var nivel = ObterNivelAlerta();
        var tempoUltimaLeitura = DateTime.UtcNow - UltimaLeituraEm;
        return $"[{ObterTipo()}] {Nome} | Tipo: {TipoSensor} | " +
               $"Local: ({Latitude:F4}, {Longitude:F4}) | " +
               $"Última leitura: {UltimaLeitura} (há {tempoUltimaLeitura.TotalMinutes:F1} min) | " +
               $"Nível: {nivel}";
    }
}
