using GestaoReservas.ConsoleClient;
using GestaoReservas.ConsoleClient.Menus;
using GestaoReservas.Domain.Dtos.Usuarios;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5001/";
var api = new ApiClient(baseUrl);

while (true)
{
    // Sem sessão (início ou token expirado/recusado): pede login antes do menu principal.
    if (api.UsuarioLogado is null && !await AutenticarAsync(api, baseUrl))
    {
        return;
    }

    ConsoleInput.LimparTela();
    Console.WriteLine("=================================================");
    Console.WriteLine(" Gestão de Reservas de Locais e Recursos - Cliente");
    Console.WriteLine($" API: {baseUrl}");
    Console.WriteLine($" Usuário: {api.UsuarioLogado!.Nome} ({api.UsuarioLogado.Email})");
    Console.WriteLine("=================================================");
    Console.WriteLine("1. Usuários");
    Console.WriteLine("2. Categorias");
    Console.WriteLine("3. Locais");
    Console.WriteLine("4. Recursos");
    Console.WriteLine("5. Reservas");
    Console.WriteLine("9. Sair da conta");
    Console.WriteLine("0. Encerrar");

    try
    {
        switch (ConsoleInput.LerTexto("Escolha uma opção"))
        {
            case "1": await UsuariosMenu.ExibirAsync(api); break;
            case "2": await CategoriasMenu.ExibirAsync(api); break;
            case "3": await LocaisMenu.ExibirAsync(api); break;
            case "4": await RecursosMenu.ExibirAsync(api); break;
            case "5": await ReservasMenu.ExibirAsync(api); break;
            case "9": api.Sair(); break;
            case "0":
                return;
            default:
                ConsoleInput.MostrarErro("Opção inválida.");
                ConsoleInput.Pausar();
                break;
        }
    }
    catch (ApiException ex)
    {
        ConsoleInput.MostrarErro(ex.Message);
        ConsoleInput.Pausar();
    }
}

// Tela de acesso: login ou criação de conta (rota pública da API). Retorna false se a pessoa escolher encerrar.
static async Task<bool> AutenticarAsync(ApiClient api, string baseUrl)
{
    while (true)
    {
        ConsoleInput.LimparTela();
        Console.WriteLine("=================================================");
        Console.WriteLine(" Gestão de Reservas - Acesso");
        Console.WriteLine($" API: {baseUrl}");
        Console.WriteLine("=================================================");
        Console.WriteLine("1. Entrar");
        Console.WriteLine("2. Criar conta");
        Console.WriteLine("0. Encerrar");

        try
        {
            switch (ConsoleInput.LerTexto("Escolha uma opção"))
            {
                case "1":
                    var email = ConsoleInput.LerTexto("E-mail");
                    var senha = ConsoleInput.LerSenha("Senha");
                    await api.EntrarAsync(email, senha);
                    return true;
                case "2":
                    var nome = ConsoleInput.LerTexto("Nome");
                    var novoEmail = ConsoleInput.LerTexto("E-mail");
                    var novaSenha = ConsoleInput.LerSenha("Senha (mínimo 4 caracteres)");
                    await api.CriarAsync<UsuarioDto>("api/usuarios", new CriarUsuarioDto(nome, novoEmail, novaSenha));
                    await api.EntrarAsync(novoEmail, novaSenha);
                    return true;
                case "0":
                    return false;
                default:
                    ConsoleInput.MostrarErro("Opção inválida.");
                    ConsoleInput.Pausar();
                    break;
            }
        }
        catch (ApiException ex)
        {
            ConsoleInput.MostrarErro(ex.Message);
            ConsoleInput.Pausar();
        }
    }
}
