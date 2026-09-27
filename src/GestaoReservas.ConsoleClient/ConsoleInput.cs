using System.Globalization;
using GestaoReservas.Domain.Enums;

namespace GestaoReservas.ConsoleClient;

/// <summary>Prompts de leitura e mensagens padronizadas para o menu de terminal.</summary>
public static class ConsoleInput
{
    public static string LerTexto(string rotulo, bool obrigatorio = true)
    {
        while (true)
        {
            Console.Write($"{rotulo}: ");
            var valor = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!obrigatorio || !string.IsNullOrWhiteSpace(valor))
            {
                return valor;
            }

            MostrarErro("Este campo é obrigatório.");
        }
    }

    public static int LerInteiro(string rotulo)
    {
        while (true)
        {
            Console.Write($"{rotulo}: ");
            if (int.TryParse(Console.ReadLine(), out var valor))
            {
                return valor;
            }

            MostrarErro("Digite um número inteiro válido.");
        }
    }

    public static int? LerInteiroOpcional(string rotulo)
    {
        while (true)
        {
            Console.Write($"{rotulo} (ENTER para deixar em branco): ");
            var texto = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            if (int.TryParse(texto, out var valor))
            {
                return valor;
            }

            MostrarErro("Digite um número inteiro válido ou deixe em branco.");
        }
    }

    public static List<int>? LerListaDeInteiros(string rotulo)
    {
        Console.Write($"{rotulo} (ids separados por vírgula, ENTER para nenhum): ");
        var texto = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        var ids = texto.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(parte => int.TryParse(parte, out _))
            .Select(int.Parse)
            .ToList();

        return ids.Count > 0 ? ids : null;
    }

    public static DateOnly LerData(string rotulo)
    {
        while (true)
        {
            Console.Write($"{rotulo} (dd/MM/yyyy): ");
            if (DateOnly.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            {
                return data;
            }

            MostrarErro("Data inválida. Use o formato dd/MM/yyyy.");
        }
    }

    public static TimeOnly LerHora(string rotulo)
    {
        while (true)
        {
            Console.Write($"{rotulo} (HH:mm): ");
            if (TimeOnly.TryParseExact(Console.ReadLine(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
            {
                return hora;
            }

            MostrarErro("Hora inválida. Use o formato HH:mm.");
        }
    }

    public static bool LerBool(string rotulo)
    {
        while (true)
        {
            Console.Write($"{rotulo} (s/n): ");
            var resposta = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (resposta is "s" or "sim")
            {
                return true;
            }

            if (resposta is "n" or "nao" or "não")
            {
                return false;
            }

            MostrarErro("Responda com 's' ou 'n'.");
        }
    }

    public static bool Confirmar(string mensagem) => LerBool(mensagem);

    public static TipoCategoria LerTipoCategoria(string rotulo)
    {
        while (true)
        {
            Console.Write($"{rotulo} (1-Local, 2-Recurso): ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    return TipoCategoria.Local;
                case "2":
                    return TipoCategoria.Recurso;
                default:
                    MostrarErro("Opção inválida.");
                    break;
            }
        }
    }

    /// <summary>Limpa a tela; não falha quando a saída não é um console real (ex.: redirecionada).</summary>
    public static void LimparTela()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
        }
    }

    public static void MostrarErro(string mensagem) => EscreverColorido(mensagem, ConsoleColor.Red);

    public static void MostrarSucesso(string mensagem) => EscreverColorido(mensagem, ConsoleColor.Green);

    public static void Pausar()
    {
        Console.WriteLine();
        Console.WriteLine("Pressione ENTER para continuar...");
        Console.ReadLine();
    }

    private static void EscreverColorido(string mensagem, ConsoleColor cor)
    {
        var corOriginal = Console.ForegroundColor;
        Console.ForegroundColor = cor;
        Console.WriteLine(mensagem);
        Console.ForegroundColor = corOriginal;
    }
}
