using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.WebApi.Dtos.Recursos;

/// <summary>Dados para atualizar um recurso existente.</summary>
/// <param name="NumeroPatrimonio">Número de patrimônio, único no sistema (até 50 caracteres).</param>
/// <param name="Descricao">Descrição do recurso (até 200 caracteres).</param>
/// <param name="DiasMinimosReserva">Duração mínima, em dias, de uma reserva do recurso (mínimo 1).</param>
/// <param name="DiasMaximosReserva">Duração máxima, em dias, de uma reserva do recurso (mínimo 1; não pode ser menor que o mínimo).</param>
/// <param name="CategoriaId">Categoria existente, ativa e do tipo Recurso.</param>
public record AtualizarRecursoDto(
    [Required, MaxLength(50)] string NumeroPatrimonio,
    [Required, MaxLength(100)] string Descricao,
    [Range(1, int.MaxValue)] int DiasMinimosReserva,
    [Range(1, int.MaxValue)] int DiasMaximosReserva,
    [Range(1, int.MaxValue)] int CategoriaId);
