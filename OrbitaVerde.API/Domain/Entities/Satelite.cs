using OrbitaVerde.API.Domain.Enums;

namespace OrbitaVerde.API.Domain.Entities;

/// <summary>
/// Representa um satélite de monitoramento ambiental.
/// Herda de EquipamentoEspacial (herança concreta).
/// </summary>
public class Satelite : EquipamentoEspacial
{
    public double AltitudeOrbitaKm { get; set; }
    public string TipoOrbita { get; set; } = string.Empty; // LEO, MEO, GEO
    public double CoberturaDegraus { get; set; }           // graus de cobertura
    public int QuantidadeSensores { get; set; }
    public ICollection<RegiaoAtiva> RegioesMonitoradas { get; set; } = new List<RegiaoAtiva>();

    public override string ObterTipo() => "Satélite";

    /// <summary>
    /// Satélites em órbita baixa monitoram com maior resolução — nível muda conforme altitude.
    /// </summary>
    public override NivelAlerta ObterNivelAlerta()
    {
        if (!EstaAtivo) return NivelAlerta.PERIGO;
        return AltitudeOrbitaKm < 500 ? NivelAlerta.NORMAL : NivelAlerta.ALERTA;
    }

    public override string RealizarMonitoramento()
    {
        var nivel = ObterNivelAlerta();
        return $"[{ObterTipo()}] {Nome} | Órbita: {TipoOrbita} a {AltitudeOrbitaKm} km | " +
               $"Sensores: {QuantidadeSensores} | Nível: {nivel} | " +
               $"Regiões monitoradas: {RegioesMonitoradas.Count}";
    }
}
