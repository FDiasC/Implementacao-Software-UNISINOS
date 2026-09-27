namespace GestaoReservas.ConsoleClient;

/// <summary>Erro retornado pela API (400/404/409/...) ou falha de conexão, já com uma mensagem pronta para exibir ao usuário.</summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}
