using GestaoReservas.Domain.Dtos.Reservas;

namespace GestaoReservas.ConsoleClient.Menus;

/// <summary>Menu de inclusão, consulta, alteração e cancelamento de reservas.</summary>
public static class ReservasMenu
{
    public static async Task ExibirAsync(ApiClient api)
    {
        while (true)
        {
            ConsoleInput.LimparTela();
            Console.WriteLine("=== Reservas ===");
            Console.WriteLine("1. Listar");
            Console.WriteLine("2. Buscar por id");
            Console.WriteLine("3. Cadastrar");
            Console.WriteLine("4. Atualizar");
            Console.WriteLine("5. Cancelar (desativar)");
            Console.WriteLine("0. Voltar");

            try
            {
                switch (ConsoleInput.LerTexto("Escolha uma opção"))
                {
                    case "1": await ListarAsync(api); break;
                    case "2": await ObterPorIdAsync(api); break;
                    case "3": await CriarAsync(api); break;
                    case "4": await AtualizarAsync(api); break;
                    case "5": await DesativarAsync(api); break;
                    case "0": return;
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

    private static async Task ListarAsync(ApiClient api)
    {
        var incluirInativas = ConsoleInput.LerBool("Incluir canceladas");
        var reservas = await api.ListarAsync<ReservaDto>($"api/reservas?apenasAtivos={!incluirInativas}");

        Console.WriteLine();
        if (reservas.Count == 0)
        {
            Console.WriteLine("Nenhuma reserva encontrada.");
        }

        foreach (var reserva in reservas)
        {
            Console.WriteLine(ResumirReserva(reserva));
        }

        ConsoleInput.Pausar();
    }

    private static async Task ObterPorIdAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id da reserva");
        var reserva = await api.ObterAsync<ReservaDto>($"api/reservas/{id}");

        Console.WriteLine();
        Console.WriteLine($"Id: {reserva.Id}");
        Console.WriteLine($"Usuário: {reserva.UsuarioId}");
        Console.WriteLine($"Local: {(reserva.LocalId?.ToString() ?? "nenhum")}");
        Console.WriteLine($"Recursos: {(reserva.RecursosId.Any() ? string.Join(", ", reserva.RecursosId) : "nenhum")}");
        Console.WriteLine($"Início: {reserva.DataInicial:dd/MM/yyyy} {reserva.HoraInicial:HH\\:mm}");
        Console.WriteLine($"Término: {reserva.DataFinal:dd/MM/yyyy} {reserva.HoraFinal:HH\\:mm}");
        Console.WriteLine($"Status: {(reserva.Ativo ? "Ativa" : "Cancelada")}");
        ConsoleInput.Pausar();
    }

    private static async Task CriarAsync(ApiClient api)
    {
        var usuarioId = ConsoleInput.LerInteiro("Id do usuário");
        var localId = ConsoleInput.LerInteiroOpcional("Id do local");
        var dataInicial = ConsoleInput.LerData("Data inicial");
        var horaInicial = ConsoleInput.LerHora("Hora inicial");
        var dataFinal = ConsoleInput.LerData("Data final");
        var horaFinal = ConsoleInput.LerHora("Hora final");
        var recursosId = ConsoleInput.LerListaDeInteiros("Ids dos recursos");

        var dto = new CriarReservaDto(usuarioId, localId, dataInicial, horaInicial, dataFinal, horaFinal, recursosId);
        var criada = await api.CriarAsync<ReservaDto>("api/reservas", dto);
        ConsoleInput.MostrarSucesso($"Reserva criada com id {criada.Id}.");
        ConsoleInput.Pausar();
    }

    private static async Task AtualizarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id da reserva a atualizar");
        var localId = ConsoleInput.LerInteiroOpcional("Novo id do local");
        var dataInicial = ConsoleInput.LerData("Nova data inicial");
        var horaInicial = ConsoleInput.LerHora("Nova hora inicial");
        var dataFinal = ConsoleInput.LerData("Nova data final");
        var horaFinal = ConsoleInput.LerHora("Nova hora final");
        var recursosId = ConsoleInput.LerListaDeInteiros("Ids dos recursos (substitui a lista atual)");

        var dto = new AtualizarReservaDto(localId, dataInicial, horaInicial, dataFinal, horaFinal, recursosId);
        await api.AtualizarAsync<ReservaDto>($"api/reservas/{id}", dto);
        ConsoleInput.MostrarSucesso("Reserva atualizada.");
        ConsoleInput.Pausar();
    }

    private static async Task DesativarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id da reserva a cancelar");
        if (!ConsoleInput.Confirmar($"Confirma o cancelamento da reserva {id}?"))
        {
            return;
        }

        await api.DesativarAsync($"api/reservas/{id}");
        ConsoleInput.MostrarSucesso("Reserva cancelada.");
        ConsoleInput.Pausar();
    }

    private static string ResumirReserva(ReservaDto reserva)
    {
        var local = reserva.LocalId?.ToString() ?? "nenhum";
        var recursos = reserva.RecursosId.Any() ? string.Join(",", reserva.RecursosId) : "nenhum";
        return $"[{reserva.Id}] Usuário {reserva.UsuarioId} - Local {local} - Recursos [{recursos}] " +
               $"- {reserva.DataInicial:dd/MM/yyyy} {reserva.HoraInicial:HH\\:mm} até {reserva.DataFinal:dd/MM/yyyy} {reserva.HoraFinal:HH\\:mm} " +
               $"- {(reserva.Ativo ? "Ativa" : "Cancelada")}";
    }
}
