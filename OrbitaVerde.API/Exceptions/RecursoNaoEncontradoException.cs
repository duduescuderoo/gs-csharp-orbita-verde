namespace OrbitaVerde.API.Exceptions;

/// <summary>
/// Exceção lançada quando um recurso solicitado não existe no banco de dados.
/// Exceção específica (não genérica) conforme requisito da disciplina.
/// </summary>
public class RecursoNaoEncontradoException : Exception
{
    public string NomeRecurso { get; }
    public object Identificador { get; }

    public RecursoNaoEncontradoException(string nomeRecurso, object identificador)
        : base($"{nomeRecurso} com identificador '{identificador}' não foi encontrado.")
    {
        NomeRecurso = nomeRecurso;
        Identificador = identificador;
    }

    public RecursoNaoEncontradoException(string nomeRecurso, object identificador, string mensagemAdicional)
        : base($"{nomeRecurso} com identificador '{identificador}' não foi encontrado. {mensagemAdicional}")
    {
        NomeRecurso = nomeRecurso;
        Identificador = identificador;
    }
}
