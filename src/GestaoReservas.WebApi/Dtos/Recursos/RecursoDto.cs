namespace GestaoReservas.WebApi.Dtos.Recursos;

/// <summary>Dados de um recurso retornados pela API.</summary>
/// <param name="Id">Identificador do recurso.</param>
/// <param name="Descricao">Descrição do recurso.</param>
/// <param name="NumeroPatrimonio">Número de patrimônio (único no sistema).</param>
/// <param name="DiasMinimosReserva">Duração mínima, em dias, de uma reserva do recurso.</param>
/// <param name="DiasMaximosReserva">Duração máxima, em dias, de uma reserva do recurso.</param>
/// <param name="CategoriaId">Identificador da categoria do recurso.</param>
/// <param name="CategoriaNome">Nome da categoria do recurso.</param>
/// <param name="Ativo">Indica se o recurso está ativo (false = excluído logicamente).</param>
public record RecursoDto(
    int Id,
    string Descricao,
    string NumeroPatrimonio,
    int DiasMinimosReserva,
    int DiasMaximosReserva,
    int CategoriaId,
    string CategoriaNome,
    bool Ativo);
