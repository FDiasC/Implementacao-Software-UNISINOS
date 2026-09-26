using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.WebApi.Dtos.Recursos;
public record AtualizarRecursoDto(
    [Required, MaxLength(100)] string NumeroPatrimonio,
    [MaxLength(100)] string Descricao,
    [Range(1, int.MaxValue)] int DiasMinimosReserva,
    [Range(1, int.MaxValue)] int DiasMaximosReserva,
    [Range(1, int.MaxValue)] int CategoriaId);