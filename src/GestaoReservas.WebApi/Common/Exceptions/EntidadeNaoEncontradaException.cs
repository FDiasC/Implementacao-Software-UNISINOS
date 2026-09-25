namespace GestaoReservas.WebApi.Common.Exceptions;

public class EntidadeNaoEncontradaException : Exception
{
    public EntidadeNaoEncontradaException(string entidade, object id)
        : base($"{entidade} com id '{id}' não foi encontrado(a).")
    {
    }
}
