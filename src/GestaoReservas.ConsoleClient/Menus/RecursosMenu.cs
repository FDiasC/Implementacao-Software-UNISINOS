using GestaoReservas.Domain.Dtos.Recursos;

namespace GestaoReservas.ConsoleClient.Menus;

/// <summary>Menu de inclusão, consulta, alteração e exclusão de recursos.</summary>
public static class RecursosMenu
{
    public static async Task ExibirAsync(ApiClient api)
    {
        while (true)
        {
            ConsoleInput.LimparTela();
            Console.WriteLine("=== Recursos ===");
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
        var recursos = await api.ListarAsync<RecursoDto>($"api/recursos?apenasAtivos={!incluirInativos}");

        Console.WriteLine();
        if (recursos.Count == 0)
        {
            Console.WriteLine("Nenhum recurso encontrado.");
        }

        foreach (var recurso in recursos)
        {
            Console.WriteLine(
                $"[{recurso.Id}] Patrimônio {recurso.NumeroPatrimonio} - {recurso.Descricao} - Categoria: {recurso.CategoriaNome} " +
                $"- Dias (min/max): {recurso.DiasMinimosReserva}/{recurso.DiasMaximosReserva} - {(recurso.Ativo ? "Ativo" : "Inativo")}");
        }

        ConsoleInput.Pausar();
    }

    private static async Task ObterPorIdAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do recurso");
        var recurso = await api.ObterAsync<RecursoDto>($"api/recursos/{id}");

        Console.WriteLine();
        Console.WriteLine($"Id: {recurso.Id}");
        Console.WriteLine($"Número de patrimônio: {recurso.NumeroPatrimonio}");
        Console.WriteLine($"Descrição: {recurso.Descricao}");
        Console.WriteLine($"Dias mínimos de reserva: {recurso.DiasMinimosReserva}");
        Console.WriteLine($"Dias máximos de reserva: {recurso.DiasMaximosReserva}");
        Console.WriteLine($"Categoria: [{recurso.CategoriaId}] {recurso.CategoriaNome}");
        Console.WriteLine($"Status: {(recurso.Ativo ? "Ativo" : "Inativo")}");
        ConsoleInput.Pausar();
    }

    private static async Task CriarAsync(ApiClient api)
    {
        var numeroPatrimonio = ConsoleInput.LerTexto("Número de patrimônio");
        var descricao = ConsoleInput.LerTexto("Descrição");
        var diasMinimos = ConsoleInput.LerInteiro("Dias mínimos de reserva");
        var diasMaximos = ConsoleInput.LerInteiro("Dias máximos de reserva");
        var categoriaId = ConsoleInput.LerInteiro("Id da categoria (tipo Recurso)");

        var dto = new CriarRecursoDto(numeroPatrimonio, descricao, diasMinimos, diasMaximos, categoriaId);
        var criado = await api.CriarAsync<RecursoDto>("api/recursos", dto);
        ConsoleInput.MostrarSucesso($"Recurso criado com id {criado.Id}.");
        ConsoleInput.Pausar();
    }

    private static async Task AtualizarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do recurso a atualizar");
        var numeroPatrimonio = ConsoleInput.LerTexto("Novo número de patrimônio");
        var descricao = ConsoleInput.LerTexto("Nova descrição");
        var diasMinimos = ConsoleInput.LerInteiro("Novos dias mínimos de reserva");
        var diasMaximos = ConsoleInput.LerInteiro("Novos dias máximos de reserva");
        var categoriaId = ConsoleInput.LerInteiro("Id da categoria (tipo Recurso)");

        var dto = new AtualizarRecursoDto(numeroPatrimonio, descricao, diasMinimos, diasMaximos, categoriaId);
        await api.AtualizarAsync<RecursoDto>($"api/recursos/{id}", dto);
        ConsoleInput.MostrarSucesso("Recurso atualizado.");
        ConsoleInput.Pausar();
    }

    private static async Task DesativarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do recurso a desativar");
        if (!ConsoleInput.Confirmar($"Confirma a desativação do recurso {id}?"))
        {
            return;
        }

        await api.DesativarAsync($"api/recursos/{id}");
        ConsoleInput.MostrarSucesso("Recurso desativado.");
        ConsoleInput.Pausar();
    }
}
