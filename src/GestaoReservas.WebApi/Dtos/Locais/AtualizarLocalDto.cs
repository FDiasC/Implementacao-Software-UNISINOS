using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.WebApi.Dtos.Locais;

/// <summary>Dados para atualizar um local existente.</summary>
/// <param name="Sala">Identificação da sala (até 100 caracteres).</param>
/// <param name="Predio">Prédio onde a sala fica (até 100 caracteres).</param>
/// <param name="Capacidade">Número máximo de pessoas (mínimo 1).</param>
/// <param name="Andar">Andar do prédio (0 ou mais).</param>
/// <param name="PermiteRecursos">Se reservas deste local podem incluir recursos.</param>
/// <param name="CategoriaId">Categoria existente, ativa e do tipo Local.</param>
public record AtualizarLocalDto(
    [Required, MaxLength(100)] string Sala,
    [Required, MaxLength(100)] string Predio,
    [Range(1, int.MaxValue)] int Capacidade,
    [Range(0, int.MaxValue)] int Andar,
    bool PermiteRecursos,
    [Range(1, int.MaxValue)] int CategoriaId);
