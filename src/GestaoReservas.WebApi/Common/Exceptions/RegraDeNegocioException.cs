namespace GestaoReservas.WebApi.Common.Exceptions;

// Lançada quando uma regra de negócio (não relacionada a "não encontrado") é violada.
public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem)
    {
    }
}
