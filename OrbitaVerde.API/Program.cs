using Microsoft.EntityFrameworkCore;
using OrbitaVerde.API.Data;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// SERVIÇOS
// =============================================

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Mantém os nomes das propriedades como camelCase no JSON
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Entity Framework Core + SQLite
builder.Services.AddDbContext<OrbitaVerdeContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=orbita_verde.db"
    )
);

builder.Services.AddEndpointsApiExplorer();

// =============================================
// PIPELINE
// =============================================

var app = builder.Build();

// Garante que o banco seja criado e as migrações aplicadas na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrbitaVerdeContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Rota raiz — informações da API
app.MapGet("/", () => new
{
    api = "OrbitaVerde API",
    versao = "1.0.0",
    descricao = "API de monitoramento ambiental satelital — FIAP Global Solution 2026",
    iniciadaEm = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss UTC"),
    endpoints = new[]
    {
        "GET  /api/satelites",
        "GET  /api/satelites/{id}",
        "POST /api/satelites",
        "PUT  /api/satelites/{id}",
        "DEL  /api/satelites/{id}",
        "GET  /api/satelites/monitoramento",
        "GET  /api/sensores",
        "GET  /api/sensores/{id}",
        "POST /api/sensores",
        "PUT  /api/sensores/{id}",
        "DEL  /api/sensores/{id}",
        "GET  /api/regioesativas",
        "GET  /api/regioesativas/{id}",
        "POST /api/regioesativas",
        "PUT  /api/regioesativas/{id}",
        "DEL  /api/regioesativas/{id}",
        "GET  /api/alertas",
        "GET  /api/alertas/{id}",
        "GET  /api/alertas/painel",
        "POST /api/alertas/flare",
        "POST /api/alertas/queimada",
        "PATCH /api/alertas/{id}/resolver",
        "DEL  /api/alertas/{id}"
    },
    integrantes = new[]
    {
        "Davi Vieira — RM556798",
        "Luca Monteiro — RM556906",
        "Arthur Silva — RM553320",
        "Eduardo Escudero — RM556527",
        "Leonardo Munhoz — RM556824"
    }
});

app.Run();
