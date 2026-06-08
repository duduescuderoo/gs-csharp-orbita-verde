using OrbitaVerde.API.Domain.Enums;

namespace OrbitaVerde.API.Domain.Interfaces;

/// <summary>
/// Contrato para qualquer equipamento ou entidade capaz de realizar monitoramento
/// e emitir um diagnóstico de nível de alerta.
/// </summary>
public interface IMonitoravel
{
    /// <summary>Retorna o nível de alerta atual do equipamento.</summary>
    NivelAlerta ObterNivelAlerta();

    /// <summary>Executa o ciclo de monitoramento e retorna uma descrição do status.</summary>
    string RealizarMonitoramento();

    /// <summary>Indica se o equipamento está em operação no momento.</summary>
    bool EstaAtivo { get; }
}
