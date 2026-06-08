using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitaVerde.API.Data;
using OrbitaVerde.API.Domain.Entities;
using OrbitaVerde.API.Exceptions;

namespace OrbitaVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensoresController : ControllerBase
{
    private readonly OrbitaVerdeContext _context;
    private readonly ILogger<SensoresController> _logger;

    public SensoresController(OrbitaVerdeContext context, ILogger<SensoresController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        try
        {
            var sensores = await _context.SensoresSolo.AsNoTracking().ToListAsync();
            var resultado = sensores.Select(s => new
            {
                s.Id,
                s.Nome,
                s.Localizacao,
                s.TipoSensor,
                s.EstaAtivo,
                s.Latitude,
                s.Longitude,
                s.UltimaLeitura,
                UltimaLeituraEm = s.UltimaLeituraEm.ToString("dd/MM/yyyy HH:mm"),
                NivelAlerta = s.ObterNivelAlerta().ToString(),
                StatusMonitoramento = s.RealizarMonitoramento(),
                Tipo = s.ObterTipo()
            });
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar sensores");
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        try
        {
            var sensor = await _context.SensoresSolo.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (sensor is null)
                throw new RecursoNaoEncontradoException("Sensor de Solo", id);

            return Ok(new
            {
                sensor.Id,
                sensor.Nome,
                sensor.Fabricante,
                sensor.Localizacao,
                sensor.TipoSensor,
                sensor.EstaAtivo,
                sensor.Latitude,
                sensor.Longitude,
                sensor.UltimaLeitura,
                UltimaLeituraEm = sensor.UltimaLeituraEm.ToString("dd/MM/yyyy HH:mm:ss"),
                DataLancamento = sensor.DataLancamento.ToString("dd/MM/yyyy"),
                TempoEmOperacaoDias = (int)sensor.TempoEmOperacao().TotalDays,
                NivelAlerta = sensor.ObterNivelAlerta().ToString(),
                StatusMonitoramento = sensor.RealizarMonitoramento()
            });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar sensor ID={Id}", id);
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] SensorSolo sensor)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sensor.Nome))
                return BadRequest(new { erro = "O nome do sensor é obrigatório." });

            sensor.DataCadastro = DateTime.UtcNow;
            sensor.UltimaLeituraEm = DateTime.UtcNow;

            _context.SensoresSolo.Add(sensor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = sensor.Id }, new { sensor.Id, sensor.Nome });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { erro = "Erro ao salvar sensor.", detalhe = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SensorSolo sensorAtualizado)
    {
        try
        {
            var sensor = await _context.SensoresSolo.FindAsync(id);
            if (sensor is null)
                throw new RecursoNaoEncontradoException("Sensor de Solo", id);

            sensor.Nome = sensorAtualizado.Nome;
            sensor.Localizacao = sensorAtualizado.Localizacao;
            sensor.TipoSensor = sensorAtualizado.TipoSensor;
            sensor.EstaAtivo = sensorAtualizado.EstaAtivo;
            sensor.Latitude = sensorAtualizado.Latitude;
            sensor.Longitude = sensorAtualizado.Longitude;
            sensor.UltimaLeitura = sensorAtualizado.UltimaLeitura;
            sensor.UltimaLeituraEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar sensor ID={Id}", id);
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var sensor = await _context.SensoresSolo.FindAsync(id);
            if (sensor is null)
                throw new RecursoNaoEncontradoException("Sensor de Solo", id);

            _context.SensoresSolo.Remove(sensor);
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
