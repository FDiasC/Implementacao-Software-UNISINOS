namespace GestaoReservas.WebApi.Common.Exceptions;

// Credenciais inválidas ou usuário do token desativado (resulta em 401).
public class NaoAutenticadoException : Exception
{
    public NaoAutenticadoException(string mensagem) : base(mensagem)
    {
    }
}
