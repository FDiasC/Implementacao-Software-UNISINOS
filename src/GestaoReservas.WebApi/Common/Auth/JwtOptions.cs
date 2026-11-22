namespace GestaoReservas.WebApi.Common.Auth;

/// <summary>Configuração da emissão/validação dos tokens JWT (seção "Jwt" do appsettings).</summary>
public class JwtOptions
{
    public const string Secao = "Jwt";

    /// <summary>Chave simétrica (HMAC-SHA256) usada para assinar o token. Mínimo de 32 caracteres.</summary>
    public string Chave { get; set; } = string.Empty;

    public string Emissor { get; set; } = "GestaoReservas.WebApi";

    public string Audiencia { get; set; } = "GestaoReservas.Clientes";

    /// <summary>Tempo de vida do token, em minutos.</summary>
    public int ExpiracaoMinutos { get; set; } = 480;
}
