namespace GestaoReservas.WebApi.Common.Exceptions;

// Lançada quando a operação conflita com o estado atual de outros registros (ex.: choque de horário).
public class ConflitoException : Exception
{
    public ConflitoException(string mensagem) : base(mensagem)
    {
    }
}
