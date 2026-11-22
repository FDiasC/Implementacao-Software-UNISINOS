namespace GestaoReservas.WebApi.Common.Exceptions;

// Regra de negócio violada (resulta em 400).
public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem)
    {
    }
}
