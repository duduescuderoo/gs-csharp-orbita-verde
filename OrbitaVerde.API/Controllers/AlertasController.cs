using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitaVerde.API.Data;
using OrbitaVerde.API.Domain.Entities;
using OrbitaVerde.API.Domain.Enums;
using OrbitaVerde.API.Exceptions;

namespace OrbitaVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertasController : ControllerBase
{
    private readonly OrbitaVerdeContext _context;
    private readonly ILogger<AlertasController> _logger;

    public AlertasController(OrbitaVerdeContext context, ILogger<AlertasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Lista todos os alertas (flares e queimadas).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll(
        [FromQuery] NivelAlerta? nivel = null,
        [FromQuery] bool? resolvidos = null)
    {
        try
        {
            var query = _context.Alertas
                .Include(a => a.RegiaoAtiva)
                .AsNoTracking()
                .AsQueryable();

            if (nivel.HasValue)
                query = query.Where(a => a.Nivel == nivel.Value);

            if (resolvidos.HasValue)
                query = resolvidos.Value
                    ? query.Where(a => a.ResolvidoEm != null)
                    : query.Where(a => a.ResolvidoEm == null);

            var alertas = await query.OrderByDescending(a => a.CriadoEm).ToListAsync();

            var resultado = alertas.Select(a => new
            {
                a.Id,
                a.Titulo,
                Categoria = a.ObterCategoria(),
                Nivel = a.Nivel.ToString(),
                CriadoEm = a.CriadoEm.ToString("dd/MM/yyyy HH:mm"),
                ResolvidoEm = a.ResolvidoEm?.ToString("dd/MM/yyyy HH:mm"),
                a.Resolvido,
                RegiaoNome = a.RegiaoAtiva?.Nome,
                Mensagem = a.GerarMensagem(),
                DetalhesEspecificos = a switch
                {
                    AlertaFlare f => (object)new { f.ClasseFlare, f.IntensidadeWm2, f.DuracaoMinutos, f.ProbabilidadeImpacto },
                    AlertaQueimada q => (object)new { q.AreaHectares, TempCelsius = q.TemperaturaKelvin - 273.15, q.Bioma, q.FonteDeteccao },
                    _ => new { }
                }
            });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar alertas");
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    /// <summary>Busca alerta por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        try
        {
            var alerta = await _context.Alertas
                .Include(a => a.RegiaoAtiva)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alerta is null)
                throw new RecursoNaoEncontradoException("Alerta", id);

            return Ok(new
            {
                alerta.Id,
                alerta.Titulo,
                alerta.Descricao,
                Categoria = alerta.ObterCategoria(),
                Nivel = alerta.Nivel.ToString(),
                CriadoEm = alerta.CriadoEm.ToString("dd/MM/yyyy HH:mm:ss"),
                ResolvidoEm = alerta.ResolvidoEm?.ToString("dd/MM/yyyy HH:mm:ss"),
                alerta.Resolvido,
                Mensagem = alerta.GerarMensagem(),
                Regiao = alerta.RegiaoAtiva == null ? null : new { alerta.RegiaoAtiva.Id, alerta.RegiaoAtiva.Nome }
            });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar alerta ID={Id}", id);
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    /// <summary>Cria novo alerta de flare solar.</summary>
    [HttpPost("flare")]
    public async Task<ActionResult<object>> CreateFlare([FromBody] AlertaFlare alerta)
    {
        try
        {
            if (!await _context.RegioesAtivas.AnyAsync(r => r.Id == alerta.RegiaoAtivaId))
                throw new RecursoNaoEncontradoException("Região Ativa", alerta.RegiaoAtivaId);

            alerta.CriadoEm = DateTime.UtcNow;
            _context.AlertasFlare.Add(alerta);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Alerta de Flare criado ID={Id} | Nível: {Nivel}", alerta.Id, alerta.Nivel);
            return CreatedAtAction(nameof(GetById), new { id = alerta.Id },
                new { alerta.Id, alerta.Titulo, Nivel = alerta.Nivel.ToString() });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { erro = "Erro ao salvar alerta.", detalhe = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    /// <summary>Cria novo alerta de queimada.</summary>
    [HttpPost("queimada")]
    public async Task<ActionResult<object>> CreateQueimada([FromBody] AlertaQueimada alerta)
    {
        try
        {
            if (!await _context.RegioesAtivas.AnyAsync(r => r.Id == alerta.RegiaoAtivaId))
                throw new RecursoNaoEncontradoException("Região Ativa", alerta.RegiaoAtivaId);

            alerta.CriadoEm = DateTime.UtcNow;
            _context.AlertasQueimada.Add(alerta);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Alerta de Queimada criado ID={Id} | Nível: {Nivel}", alerta.Id, alerta.Nivel);
            return CreatedAtAction(nameof(GetById), new { id = alerta.Id },
                new { alerta.Id, alerta.Titulo, Nivel = alerta.Nivel.ToString() });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { erro = "Erro ao salvar alerta.", detalhe = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    /// <summary>Marca um alerta como resolvido.</summary>
    [HttpPatch("{id:int}/resolver")]
    public async Task<IActionResult> Resolver(int id)
    {
        try
        {
            var alerta = await _context.Alertas.FindAsync(id);
            if (alerta is null)
                throw new RecursoNaoEncontradoException("Alerta", id);

            // Chama o método do domínio — lança InvalidOperationException se já resolvido
            alerta.Resolver();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Alerta ID={Id} resolvido em {Em}", id, alerta.ResolvidoEm);
            return Ok(new
            {
                mensagem = $"Alerta ID={id} resolvido com sucesso.",
                resolvidoEm = alerta.ResolvidoEm?.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Tratamento específico: alerta já estava resolvido
            _logger.LogWarning("Tentativa de resolver alerta já resolvido ID={Id}", id);
            return Conflict(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resolver alerta ID={Id}", id);
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    /// <summary>Remove um alerta.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var alerta = await _context.Alertas.FindAsync(id);
            if (alerta is null)
                throw new RecursoNaoEncontradoException("Alerta", id);

            _context.Alertas.Remove(alerta);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    /// <summary>Painel de alertas ativos com estatísticas.</summary>
    [HttpGet("painel")]
    public async Task<ActionResult<object>> Painel()
    {
        try
        {
            var alertas = await _context.Alertas
                .Include(a => a.RegiaoAtiva)
                .AsNoTracking()
                .ToListAsync();

            var abertos = alertas.Where(a => !a.Resolvido).ToList();

            return Ok(new
            {
                geradoEm = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss UTC"),
                totalAlertas = alertas.Count,
                abertos = abertos.Count,
                resolvidos = alertas.Count - abertos.Count,
                porNivel = new
                {
                    normal = abertos.Count(a => a.Nivel == NivelAlerta.NORMAL),
                    alerta = abertos.Count(a => a.Nivel == NivelAlerta.ALERTA),
                    perigo = abertos.Count(a => a.Nivel == NivelAlerta.PERIGO)
                },
                porCategoria = abertos
                    .GroupBy(a => a.ObterCategoria())
                    .Select(g => new { categoria = g.Key, total = g.Count() }),
                alertasMaisRecentes = abertos
                    .OrderByDescending(a => a.CriadoEm)
                    .Take(5)
                    .Select(a => new
                    {
                        a.Id,
                        a.Titulo,
                        Nivel = a.Nivel.ToString(),
                        Categoria = a.ObterCategoria(),
                        CriadoEm = a.CriadoEm.ToString("dd/MM/yyyy HH:mm")
                    })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar painel de alertas");
            return StatusCode(500, new { erro = ex.Message });
        }
    }
}
