using OrbitaVerde.API.Domain.Enums;
using OrbitaVerde.API.Domain.Interfaces;

namespace OrbitaVerde.API.Domain.Entities;

/// <summary>
/// Classe base abstrata para todos os equipamentos da plataforma OrbitaVerde.
/// Implementa o contrato IMonitoravel e define o comportamento padrão de monitoramento.
/// </summary>
public abstract class EquipamentoEspacial : IMonitoravel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public DateTime DataLancamento { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool EstaAtivo { get; set; } = true;

    /// <summary>
    /// Retorna o tipo do equipamento — implementado por cada subclasse.
    /// Exemplo de método abstrato obrigando herança.
    /// </summary>
    public abstract string ObterTipo();

    /// <summary>
    /// Calcula o tempo em operação desde o lançamento.
    /// Demonstra uso de DateTime.
    /// </summary>
    public TimeSpan TempoEmOperacao()
        => DateTime.UtcNow - DataLancamento;

    /// <summary>
    /// Implementação padrão de IMonitoravel.ObterNivelAlerta.
    /// Subclasses podem sobrescrever para lógica específica.
    /// </summary>
    public virtual NivelAlerta ObterNivelAlerta()
        => EstaAtivo ? NivelAlerta.NORMAL : NivelAlerta.PERIGO;

    /// <summary>
    /// Implementação padrão de RealizarMonitoramento.
    /// </summary>
    public virtual string RealizarMonitoramento()
    {
        var nivel = ObterNivelAlerta();
        var tempoOp = TempoEmOperacao();
        return $"[{ObterTipo()}] {Nome} | Status: {(EstaAtivo ? "ATIVO" : "INATIVO")} | " +
               $"Nível: {nivel} | Em operação há: {tempoOp.Days} dias";
    }

    public override string ToString()
        => $"{ObterTipo()} '{Nome}' (ID={Id}, Fabricante={Fabricante})";
}
