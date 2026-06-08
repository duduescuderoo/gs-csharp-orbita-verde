using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitaVerde.API.Data;
using OrbitaVerde.API.Domain.Entities;
using OrbitaVerde.API.Exceptions;

namespace OrbitaVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SatelitesController : ControllerBase
{
    private readonly OrbitaVerdeContext _context;
    private readonly ILogger<SatelitesController> _logger;

    public SatelitesController(OrbitaVerdeContext context, ILogger<SatelitesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Lista todos os satélites cadastrados.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        try
        {
            var satelites = await _context.Satelites
                .Include(s => s.RegioesMonitoradas)
                .AsNoTracking()
                .ToListAsync();

            var resultado = satelites.Select(s => new
            {
                s.Id,
                s.Nome,
                s.Fabricante,
                s.EstaAtivo,
                s.TipoOrbita,
                s.AltitudeOrbitaKm,
                s.QuantidadeSensores,
                DataLancamento = s.DataLancamento.ToString("dd/MM/yyyy"),
                TempoEmOperacaoDias = (int)s.TempoEmOperacao().TotalDays,
                NivelAlerta = s.ObterNivelAlerta().ToString(),
                StatusMonitoramento = s.RealizarMonitoramento(),
                TotalRegioesMonitoradas = s.RegioesMonitoradas.Count
            });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar satélites");
            return StatusCode(500, new { erro = "Erro interno ao listar satélites.", detalhe = ex.Message });
        }
    }

    /// <summary>Busca satélite por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        try
        {
            var satelite = await _context.Satelites
                .Include(s => s.RegioesMonitoradas)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (satelite is null)
                throw new RecursoNaoEncontradoException("Satélite", id);

            return Ok(new
            {
                satelite.Id,
                satelite.Nome,
                satelite.Fabricante,
                satelite.EstaAtivo,
                satelite.TipoOrbita,
                satelite.AltitudeOrbitaKm,
                satelite.CoberturaDegraus,
                satelite.QuantidadeSensores,
                DataLancamento = satelite.DataLancamento.ToString("dd/MM/yyyy"),
                DataCadastro = satelite.DataCadastro.ToString("dd/MM/yyyy HH:mm"),
                TempoEmOperacaoDias = (int)satelite.TempoEmOperacao().TotalDays,
                NivelAlerta = satelite.ObterNivelAlerta().ToString(),
                StatusMonitoramento = satelite.RealizarMonitoramento(),
                Tipo = satelite.ObterTipo(),
                RegioesMonitoradas = satelite.RegioesMonitoradas.Select(r => new { r.Id, r.Nome, r.NivelAtual })
            });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar satélite ID={Id}", id);
            return StatusCode(500, new { erro = "Erro interno.", detalhe = ex.Message });
        }
    }

    /// <summary>Cadastra um novo satélite.</summary>
    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] Satelite satelite)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(satelite.Nome))
                return BadRequest(new { erro = "O nome do satélite é obrigatório." });

            satelite.DataCadastro = DateTime.UtcNow;

            _context.Satelites.Add(satelite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Satélite '{Nome}' cadastrado com ID={Id}", satelite.Nome, satelite.Id);
            return CreatedAtAction(nameof(GetById), new { id = satelite.Id }, new { satelite.Id, satelite.Nome });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar satélite no banco");
            return StatusCode(500, new { erro = "Erro ao salvar no banco de dados.", detalhe = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar satélite");
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    /// <summary>Atualiza dados de um satélite.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Satelite sateliteAtualizado)
    {
        try
        {
            var satelite = await _context.Satelites.FindAsync(id);
            if (satelite is null)
                throw new RecursoNaoEncontradoException("Satélite", id);

            satelite.Nome = sateliteAtualizado.Nome;
            satelite.Fabricante = sateliteAtualizado.Fabricante;
            satelite.EstaAtivo = sateliteAtualizado.EstaAtivo;
            satelite.AltitudeOrbitaKm = sateliteAtualizado.AltitudeOrbitaKm;
            satelite.TipoOrbita = sateliteAtualizado.TipoOrbita;
            satelite.CoberturaDegraus = sateliteAtualizado.CoberturaDegraus;
            satelite.QuantidadeSensores = sateliteAtualizado.QuantidadeSensores;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Conflito de concorrência ao atualizar satélite ID={Id}", id);
            return Conflict(new { erro = "Conflito de concorrência ao atualizar o registro." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar satélite ID={Id}", id);
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    /// <summary>Remove um satélite.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var satelite = await _context.Satelites.FindAsync(id);
            if (satelite is null)
                throw new RecursoNaoEncontradoException("Satélite", id);

            _context.Satelites.Remove(satelite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Satélite ID={Id} removido", id);
            return NoContent();
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro de banco ao remover satélite ID={Id}", id);
            return StatusCode(500, new { erro = "Não foi possível remover. Verifique se há regiões vinculadas.", detalhe = ex.InnerException?.Message });
        }
    }

    /// <summary>Executa monitoramento em todos os satélites ativos.</summary>
    [HttpGet("monitoramento")]
    public async Task<ActionResult<IEnumerable<string>>> ExecutarMonitoramento()
    {
        try
        {
            var satelites = await _context.Satelites.Where(s => s.EstaAtivo).ToListAsync();
            var relatorios = satelites.Select(s => s.RealizarMonitoramento()).ToList();
            return Ok(new
            {
                executadoEm = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss UTC"),
                totalAtivos = satelites.Count,
                relatorios
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }
}
