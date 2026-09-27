using GestaoReservas.Domain.Dtos.Usuarios;

namespace GestaoReservas.ConsoleClient.Menus;

/// <summary>Menu de inclusão, consulta, alteração e exclusão de usuários.</summary>
public static class UsuariosMenu
{
    public static async Task ExibirAsync(ApiClient api)
    {
        while (true)
        {
            ConsoleInput.LimparTela();
            Console.WriteLine("=== Usuários ===");
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
        var usuarios = await api.ListarAsync<UsuarioDto>($"api/usuarios?apenasAtivos={!incluirInativos}");

        Console.WriteLine();
        if (usuarios.Count == 0)
        {
            Console.WriteLine("Nenhum usuário encontrado.");
        }

        foreach (var usuario in usuarios)
        {
            Console.WriteLine($"[{usuario.Id}] {usuario.Nome} - {usuario.Email} - {(usuario.Ativo ? "Ativo" : "Inativo")}");
        }

        ConsoleInput.Pausar();
    }

    private static async Task ObterPorIdAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do usuário");
        var usuario = await api.ObterAsync<UsuarioDto>($"api/usuarios/{id}");

        Console.WriteLine();
        Console.WriteLine($"Id: {usuario.Id}");
        Console.WriteLine($"Nome: {usuario.Nome}");
        Console.WriteLine($"E-mail: {usuario.Email}");
        Console.WriteLine($"Status: {(usuario.Ativo ? "Ativo" : "Inativo")}");
        ConsoleInput.Pausar();
    }

    private static async Task CriarAsync(ApiClient api)
    {
        var nome = ConsoleInput.LerTexto("Nome");
        var email = ConsoleInput.LerTexto("E-mail");
        var senha = ConsoleInput.LerTexto("Senha (mínimo 4 caracteres)");

        var criado = await api.CriarAsync<UsuarioDto>("api/usuarios", new CriarUsuarioDto(nome, email, senha));
        ConsoleInput.MostrarSucesso($"Usuário criado com id {criado.Id}.");
        ConsoleInput.Pausar();
    }

    private static async Task AtualizarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do usuário a atualizar");
        var nome = ConsoleInput.LerTexto("Novo nome");
        var email = ConsoleInput.LerTexto("Novo e-mail");
        var senha = ConsoleInput.LerTexto("Nova senha (ENTER para manter a atual)", obrigatorio: false);

        var dto = new AtualizarUsuarioDto(nome, email, string.IsNullOrWhiteSpace(senha) ? null : senha);
        await api.AtualizarAsync<UsuarioDto>($"api/usuarios/{id}", dto);
        ConsoleInput.MostrarSucesso("Usuário atualizado.");
        ConsoleInput.Pausar();
    }

    private static async Task DesativarAsync(ApiClient api)
    {
        var id = ConsoleInput.LerInteiro("Id do usuário a desativar");
        if (!ConsoleInput.Confirmar($"Confirma a desativação do usuário {id}?"))
        {
            return;
        }

        await api.DesativarAsync($"api/usuarios/{id}");
        ConsoleInput.MostrarSucesso("Usuário desativado.");
        ConsoleInput.Pausar();
    }
}
