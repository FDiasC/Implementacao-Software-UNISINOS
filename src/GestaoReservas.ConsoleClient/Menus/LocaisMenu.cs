using GestaoReservas.Domain.Dtos.Locais;

namespace GestaoReservas.ConsoleClient.Menus;

/// <summary>Menu de inclusão, consulta, alteração e exclusão de locais.</summary>
public static class LocaisMenu
{
    public static async Task ExibirAsync(ApiClient api)
    {
        while (true)
        {
            ConsoleInput.LimparTela();
            Console.WriteLine("=== Locais ===");
            Console.WriteLine("1. Listar");
            Console.WriteLine("2. Buscar por id");
            Console.WriteLine("3. Cadastrar");
            Console.WriteLine("4. Atualizar");
            Console.WriteLine("5. Desativar (excluir)");
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
        var incluirInativos = ConsoleInput.LerBool("Incluir inativos");
        var locais = await api.ListarAsync<LocalDto>($"api/locais?apenasAtivos={!incluirInativos}");

        Console.WriteLine();
        if (locais.Count == 0)
        {
            Console.WriteLine("Nenhum local encontrado.");
        }

        foreach (var local in locais)
        {
            Console.WriteLine(
                $"[{local.Id}] Sala {local.Sala} - Prédio {local.Predio} - Andar {local.Andar} - Capacidade {local.Capacidade} " +
                $"- Categoria: {local.CategoriaNome} - PermiteRecursos: {(local.PermiteRecursos ? "Sim" : "Não")} - {(local.Ativo ? "Ativo" : "Inativo")}");
        }

        ConsoleInput.Pausar();
    }

    private static async Task ObterPorIdAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do local");
        var local = await api.ObterAsync<LocalDto>($"api/locais/{id}");

        Console.WriteLine();
        Console.WriteLine($"Id: {local.Id}");
        Console.WriteLine($"Sala: {local.Sala}");
        Console.WriteLine($"Prédio: {local.Predio}");
        Console.WriteLine($"Capacidade: {local.Capacidade}");
        Console.WriteLine($"Andar: {local.Andar}");
        Console.WriteLine($"Permite recursos: {(local.PermiteRecursos ? "Sim" : "Não")}");
        Console.WriteLine($"Categoria: [{local.CategoriaId}] {local.CategoriaNome}");
        Console.WriteLine($"Status: {(local.Ativo ? "Ativo" : "Inativo")}");
        ConsoleInput.Pausar();
    }

    private static async Task CriarAsync(ApiClient api)
    {
        var sala = ConsoleInput.LerTexto("Sala");
        var predio = ConsoleInput.LerTexto("Prédio");
        var capacidade = ConsoleInput.LerInteiro("Capacidade");
        var andar = ConsoleInput.LerInteiro("Andar");
        var permiteRecursos = ConsoleInput.LerBool("Permite recursos");
        var categoriaId = ConsoleInput.LerInteiro("Id da categoria (tipo Local)");

        var dto = new CriarLocalDto(sala, predio, capacidade, andar, permiteRecursos, categoriaId);
        var criado = await api.CriarAsync<LocalDto>("api/locais", dto);
        ConsoleInput.MostrarSucesso($"Local criado com id {criado.Id}.");
        ConsoleInput.Pausar();
    }

    private static async Task AtualizarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do local a atualizar");
        var sala = ConsoleInput.LerTexto("Nova sala");
        var predio = ConsoleInput.LerTexto("Novo prédio");
        var capacidade = ConsoleInput.LerInteiro("Nova capacidade");
        var andar = ConsoleInput.LerInteiro("Novo andar");
        var permiteRecursos = ConsoleInput.LerBool("Permite recursos");
        var categoriaId = ConsoleInput.LerInteiro("Id da categoria (tipo Local)");

        var dto = new AtualizarLocalDto(sala, predio, capacidade, andar, permiteRecursos, categoriaId);
        await api.AtualizarAsync<LocalDto>($"api/locais/{id}", dto);
        ConsoleInput.MostrarSucesso("Local atualizado.");
        ConsoleInput.Pausar();
    }

    private static async Task DesativarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do local a desativar");
        if (!ConsoleInput.Confirmar($"Confirma a desativação do local {id}?"))
        {
            return;
        }

        await api.DesativarAsync($"api/locais/{id}");
        ConsoleInput.MostrarSucesso("Local desativado.");
        ConsoleInput.Pausar();
    }
}
