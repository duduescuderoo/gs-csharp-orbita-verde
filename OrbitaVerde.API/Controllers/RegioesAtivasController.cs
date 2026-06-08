using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitaVerde.API.Data;
using OrbitaVerde.API.Domain.Entities;
using OrbitaVerde.API.Domain.Enums;
using OrbitaVerde.API.Exceptions;

namespace OrbitaVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegioesAtivasController : ControllerBase
{
    private readonly OrbitaVerdeContext _context;
    private readonly ILogger<RegioesAtivasController> _logger;

    public RegioesAtivasController(OrbitaVerdeContext context, ILogger<RegioesAtivasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] NivelAlerta? nivel = null)
    {
        try
        {
            var query = _context.RegioesAtivas
                .Include(r => r.Alertas)
                .Include(r => r.Satelite)
                .AsNoTracking()
                .AsQueryable();

            if (nivel.HasValue)
                query = query.Where(r => r.NivelAtual == nivel.Value);

            var regioes = await query.ToListAsync();
            var resultado = regioes.Select(r => new
            {
                r.Id,
                r.Nome,
                r.Descricao,
                r.Latitude,
                r.Longitude,
                NivelAtual = r.NivelAtual.ToString(),
                PrimeiraDeteccao = r.PrimeiraDeteccao.ToString("dd/MM/yyyy HH:mm"),
                UltimaAtualizacao = r.UltimaAtualizacao.ToString("dd/MM/yyyy HH:mm"),
                TempoAtivaHoras = r.TempoDesdeDeteccao().TotalHours.ToString("F1"),
                r.Ativa,
                SateliteNome = r.Satelite?.Nome,
                TotalAlertas = r.Alertas.Count,
                AlertasAbertos = r.Alertas.Count(a => !a.Resolvido),
                Resumo = r.ToString()
            });

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = $"Parâmetro inválido: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar regiões ativas");
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        try
        {
            var regiao = await _context.RegioesAtivas
                .Include(r => r.Alertas)
                .Include(r => r.Satelite)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (regiao is null)
                throw new RecursoNaoEncontradoException("Região Ativa", id);

            return Ok(new
            {
                regiao.Id,
                regiao.Nome,
                regiao.Descricao,
                regiao.Latitude,
                regiao.Longitude,
                NivelAtual = regiao.NivelAtual.ToString(),
                PrimeiraDeteccao = regiao.PrimeiraDeteccao.ToString("dd/MM/yyyy HH:mm:ss"),
                UltimaAtualizacao = regiao.UltimaAtualizacao.ToString("dd/MM/yyyy HH:mm:ss"),
                TempoAtivaHoras = regiao.TempoDesdeDeteccao().TotalHours.ToString("F1"),
                regiao.Ativa,
                Satelite = regiao.Satelite == null ? null : new { regiao.Satelite.Id, regiao.Satelite.Nome },
                Alertas = regiao.Alertas.Select(a => new
                {
                    a.Id,
                    a.Titulo,
                    Nivel = a.Nivel.ToString(),
                    Categoria = a.ObterCategoria(),
                    a.Resolvido,
                    CriadoEm = a.CriadoEm.ToString("dd/MM/yyyy HH:mm"),
                    Mensagem = a.GerarMensagem()
                })
            });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar região ID={Id}", id);
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] RegiaoAtiva regiao)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(regiao.Nome))
                return BadRequest(new { erro = "O nome da região é obrigatório." });

            regiao.PrimeiraDeteccao = DateTime.UtcNow;
            regiao.UltimaAtualizacao = DateTime.UtcNow;

            _context.RegioesAtivas.Add(regiao);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Região ativa '{Nome}' cadastrada ID={Id}", regiao.Nome, regiao.Id);
            return CreatedAtAction(nameof(GetById), new { id = regiao.Id }, new { regiao.Id, regiao.Nome });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { erro = "Erro ao salvar região.", detalhe = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RegiaoAtiva regiaoAtualizada)
    {
        try
        {
            var regiao = await _context.RegioesAtivas.FindAsync(id);
            if (regiao is null)
                throw new RecursoNaoEncontradoException("Região Ativa", id);

            regiao.Nome = regiaoAtualizada.Nome;
            regiao.Descricao = regiaoAtualizada.Descricao;
            regiao.NivelAtual = regiaoAtualizada.NivelAtual;
            regiao.Ativa = regiaoAtualizada.Ativa;
            regiao.UltimaAtualizacao = DateTime.UtcNow;

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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var regiao = await _context.RegioesAtivas.FindAsync(id);
            if (regiao is null)
                throw new RecursoNaoEncontradoException("Região Ativa", id);

            _context.RegioesAtivas.Remove(regiao);
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
}
