using GestaoReservas.Domain.Dtos.Categorias;

namespace GestaoReservas.ConsoleClient.Menus;

/// <summary>Menu de inclusão, consulta, alteração e exclusão de categorias.</summary>
public static class CategoriasMenu
{
    public static async Task ExibirAsync(ApiClient api)
    {
        while (true)
        {
            ConsoleInput.LimparTela();
            Console.WriteLine("=== Categorias ===");
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
        var incluirInativas = ConsoleInput.LerBool("Incluir inativas");
        var categorias = await api.ListarAsync<CategoriaDto>($"api/categorias?apenasAtivas={!incluirInativas}");

        Console.WriteLine();
        if (categorias.Count == 0)
        {
            Console.WriteLine("Nenhuma categoria encontrada.");
        }

        foreach (var categoria in categorias)
        {
            Console.WriteLine($"[{categoria.Id}] {categoria.Nome} - Tipo: {categoria.Tipo} - {(categoria.Ativo ? "Ativa" : "Inativa")}");
        }

        ConsoleInput.Pausar();
    }

    private static async Task ObterPorIdAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id da categoria");
        var categoria = await api.ObterAsync<CategoriaDto>($"api/categorias/{id}");

        Console.WriteLine();
        Console.WriteLine($"Id: {categoria.Id}");
        Console.WriteLine($"Nome: {categoria.Nome}");
        Console.WriteLine($"Tipo: {categoria.Tipo}");
        Console.WriteLine($"Status: {(categoria.Ativo ? "Ativa" : "Inativa")}");
        ConsoleInput.Pausar();
    }

    private static async Task CriarAsync(ApiClient api)
    {
        var nome = ConsoleInput.LerTexto("Nome");
        var tipo = ConsoleInput.LerTipoCategoria("Tipo");

        var criada = await api.CriarAsync<CategoriaDto>("api/categorias", new CriarCategoriaDto(nome, tipo));
        ConsoleInput.MostrarSucesso($"Categoria criada com id {criada.Id}.");
        ConsoleInput.Pausar();
    }

    private static async Task AtualizarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id da categoria a atualizar");
        var nome = ConsoleInput.LerTexto("Novo nome");
        var tipo = ConsoleInput.LerTipoCategoria("Novo tipo");

        await api.AtualizarAsync<CategoriaDto>($"api/categorias/{id}", new AtualizarCategoriaDto(nome, tipo));
        ConsoleInput.MostrarSucesso("Categoria atualizada.");
        ConsoleInput.Pausar();
    }

    private static async Task DesativarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id da categoria a desativar");
        if (!ConsoleInput.Confirmar($"Confirma a desativação da categoria {id}?"))
        {
            return;
        }

        await api.DesativarAsync($"api/categorias/{id}");
        ConsoleInput.MostrarSucesso("Categoria desativada.");
        ConsoleInput.Pausar();
    }
}
