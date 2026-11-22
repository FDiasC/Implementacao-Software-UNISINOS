using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GestaoReservas.Domain.Dtos.Auth;
using GestaoReservas.Domain.Dtos.Usuarios;

namespace GestaoReservas.ConsoleClient;

/// <summary>Cliente HTTP fino para consumir a GestaoReservas.WebApi, convertendo erros em <see cref="ApiException"/>.</summary>
public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    /// <summary>Usuário dono do token atual; nulo quando não há sessão (antes do login ou após um 401).</summary>
    public UsuarioDto? UsuarioLogado { get; private set; }

    /// <summary>Faz login e passa a enviar o token JWT em todas as requisições.</summary>
    public async Task EntrarAsync(string email, string senha, CancellationToken ct = default)
    {
        var resposta = await EnviarAsync(HttpMethod.Post, "api/auth/token", new LoginDto(email, senha), ct);
        var token = (await resposta.Content.ReadFromJsonAsync<TokenDto>(JsonOptions, ct))!;

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        UsuarioLogado = token.Usuario;
    }

    public void Sair()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        UsuarioLogado = null;
    }

    public async Task<List<T>> ListarAsync<T>(string caminho, CancellationToken ct = default)
    {
        var resposta = await EnviarAsync(HttpMethod.Get, caminho, ct: ct);
        return (await resposta.Content.ReadFromJsonAsync<List<T>>(JsonOptions, ct))!;
    }

    public async Task<T> ObterAsync<T>(string caminho, CancellationToken ct = default)
    {
        var resposta = await EnviarAsync(HttpMethod.Get, caminho, ct: ct);
        return (await resposta.Content.ReadFromJsonAsync<T>(JsonOptions, ct))!;
    }

    public async Task<T> CriarAsync<T>(string caminho, object corpo, CancellationToken ct = default)
    {
        var resposta = await EnviarAsync(HttpMethod.Post, caminho, corpo, ct);
        return (await resposta.Content.ReadFromJsonAsync<T>(JsonOptions, ct))!;
    }

    public async Task<T> AtualizarAsync<T>(string caminho, object corpo, CancellationToken ct = default)
    {
        var resposta = await EnviarAsync(HttpMethod.Put, caminho, corpo, ct);
        return (await resposta.Content.ReadFromJsonAsync<T>(JsonOptions, ct))!;
    }

    public async Task DesativarAsync(string caminho, CancellationToken ct = default) =>
        await EnviarAsync(HttpMethod.Delete, caminho, ct: ct);

    private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string caminho, object? corpo = null, CancellationToken ct = default)
    {
        using var requisicao = new HttpRequestMessage(metodo, caminho);
        if (corpo is not null)
        {
            requisicao.Content = JsonContent.Create(corpo, options: JsonOptions);
        }

        HttpResponseMessage resposta;
        try
        {
            resposta = await _http.SendAsync(requisicao, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(
                $"Não foi possível conectar à API em {_http.BaseAddress}. Verifique se ela está em execução. Detalhe: {ex.Message}");
        }

        if (resposta.IsSuccessStatusCode)
        {
            return resposta;
        }

        var mensagem = await ExtrairMensagemDeErroAsync(resposta, ct);

        // Token ausente/expirado ou usuário desativado: encerra a sessão para o menu pedir login de novo.
        if (resposta.StatusCode == HttpStatusCode.Unauthorized)
        {
            Sair();
        }

        throw new ApiException(mensagem);
    }

    private static async Task<string> ExtrairMensagemDeErroAsync(HttpResponseMessage resposta, CancellationToken ct)
    {
        try
        {
            var problema = await resposta.Content.ReadFromJsonAsync<ProblemaDto>(JsonOptions, ct);

            // Erro de validação automática do ModelState (400): o título é sempre genérico
            // ("One or more validation errors occurred."); a causa real vem em "errors", por campo.
            if (problema?.Errors is { Count: > 0 })
            {
                var detalhes = problema.Errors
                    .SelectMany(campo => campo.Value.Select(mensagem => $"{campo.Key}: {mensagem}"));
                return string.Join(" | ", detalhes);
            }

            if (!string.IsNullOrWhiteSpace(problema?.Title))
            {
                return problema!.Title!;
            }
        }
        catch (JsonException)
        {
            // corpo não é um ProblemDetails; cai no retorno genérico abaixo.
        }

        return $"A API retornou o erro HTTP {(int)resposta.StatusCode} ({resposta.StatusCode}).";
    }

    private record ProblemaDto(string? Title, int? Status, Dictionary<string, string[]>? Errors);
}
