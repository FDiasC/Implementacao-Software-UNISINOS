namespace GestaoReservas.WebApi.Common.Exceptions;

// Lançada quando as credenciais ou o usuário do token não são válidos (resulta em 401).
public class NaoAutenticadoException : Exception
{
    public NaoAutenticadoException(string mensagem) : base(mensagem)
    {
    }
}
