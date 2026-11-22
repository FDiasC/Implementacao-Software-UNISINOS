namespace GestaoReservas.Domain.Dtos.Recursos;

/// <summary>Dados de um recurso retornados pela API.</summary>
public record RecursoDto(
    int Id,
    string Descricao,
    string NumeroPatrimonio,
    int DiasMinimosReserva,
    int DiasMaximosReserva,
    int CategoriaId,
    string CategoriaNome,
    bool Ativo);
