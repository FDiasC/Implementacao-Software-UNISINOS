namespace GestaoReservas.WebApi.Common.Exceptions;

// Operação conflita com outro registro, ex.: choque de horário (resulta em 409).
public class ConflitoException : Exception
{
    public ConflitoException(string mensagem) : base(mensagem)
    {
    }
}
