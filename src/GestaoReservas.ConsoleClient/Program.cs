using GestaoReservas.ConsoleClient;
using GestaoReservas.ConsoleClient.Menus;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:3000/";
var api = new ApiClient(baseUrl);

while (true)
{
    ConsoleInput.LimparTela();
    Console.WriteLine("=================================================");
    Console.WriteLine(" Gestão de Reservas de Locais e Recursos - Cliente");
    Console.WriteLine($" API: {baseUrl}");
    Console.WriteLine("=================================================");
    Console.WriteLine("1. Usuários");
    Console.WriteLine("2. Categorias");
    Console.WriteLine("3. Locais");
    Console.WriteLine("4. Recursos");
    Console.WriteLine("5. Reservas");
    Console.WriteLine("0. Sair");

    try
    {
        switch (ConsoleInput.LerTexto("Escolha uma opção"))
        {
            case "1": await UsuariosMenu.ExibirAsync(api); break;
            case "2": await CategoriasMenu.ExibirAsync(api); break;
            case "3": await LocaisMenu.ExibirAsync(api); break;
            case "4": await RecursosMenu.ExibirAsync(api); break;
            case "5": await ReservasMenu.ExibirAsync(api); break;
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
