using OrbitaVerde.API.Domain.Enums;

namespace OrbitaVerde.API.Domain.Entities;

/// <summary>
/// Classe base abstrata para todos os tipos de alerta da plataforma.
/// Demonstra abstração e herança — AlertaFlare e AlertaQueimada a estendem.
/// </summary>
public abstract class Alerta
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public NivelAlerta Nivel { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvidoEm { get; set; }
    public bool Resolvido => ResolvidoEm.HasValue;

    // FK
    public int RegiaoAtivaId { get; set; }
    public RegiaoAtiva? RegiaoAtiva { get; set; }

    /// <summary>
    /// Cada tipo de alerta define sua categoria textual.
    /// </summary>
    public abstract string ObterCategoria();

    /// <summary>
    /// Gera o texto completo do alerta para notificação.
    /// </summary>
    public virtual string GerarMensagem()
    {
        var status = Resolvido
            ? $"RESOLVIDO em {ResolvidoEm!.Value:dd/MM/yyyy HH:mm}"
            : $"ABERTO há {(DateTime.UtcNow - CriadoEm).TotalMinutes:F0} min";

        return $"[{ObterCategoria().ToUpper()}] {Titulo} | Nível: {Nivel} | {status}";
    }

    /// <summary>
    /// Marca o alerta como resolvido, registrando o momento exato (DateTime).
    /// </summary>
    public void Resolver()
    {
        if (Resolvido)
            throw new InvalidOperationException($"Alerta ID={Id} já está resolvido desde {ResolvidoEm}.");

        ResolvidoEm = DateTime.UtcNow;
    }
}
