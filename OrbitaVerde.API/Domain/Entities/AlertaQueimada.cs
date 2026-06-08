namespace OrbitaVerde.API.Domain.Entities;

/// <summary>
/// Alerta específico para detecção de queimadas via satélite ou sensor de solo.
/// Segunda subclasse concreta de Alerta.
/// </summary>
public class AlertaQueimada : Alerta
{
    /// <summary>Área estimada afetada em hectares.</summary>
    public double AreaHectares { get; set; }

    /// <summary>Temperatura superficial detectada em Kelvin.</summary>
    public double TemperaturaKelvin { get; set; }

    /// <summary>Bioma afetado (ex: Cerrado, Amazônia, Mata Atlântica).</summary>
    public string Bioma { get; set; } = string.Empty;

    /// <summary>Fonte de detecção (SATELLITE, SENSOR_SOLO, AMBOS).</summary>
    public string FonteDeteccao { get; set; } = "SATELLITE";

    public override string ObterCategoria() => "Queimada";

    public override string GerarMensagem()
    {
        var base_ = base.GerarMensagem();
        double tempCelsius = TemperaturaKelvin - 273.15;
        return $"{base_} | Área: {AreaHectares:F1} ha | " +
               $"Temp: {tempCelsius:F1}°C | Bioma: {Bioma} | Fonte: {FonteDeteccao}";
    }
}
