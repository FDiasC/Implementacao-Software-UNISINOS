namespace GestaoReservas.WebApi.Dtos.Recursos;

public record RecursoDto(
    int Id,
    string Descricao,
    string NumeroPatrimonio,
    int DiasMinimosReserva,
    int DiasMaximosReserva,
    int CategoriaId,
    string CategoriaNome,
    bool Ativo);