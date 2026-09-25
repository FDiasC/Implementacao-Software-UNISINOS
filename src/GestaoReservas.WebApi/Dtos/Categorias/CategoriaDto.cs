namespace GestaoReservas.WebApi.Dtos.Categorias;

/// <summary>Dados de uma categoria retornados pela API.</summary>
/// <param name="Id">Identificador da categoria.</param>
/// <param name="Nome">Nome da categoria.</param>
/// <param name="Tipo">A que entidade a categoria se aplica: Local ou Recurso.</param>
/// <param name="Ativo">Indica se a categoria está ativa (false = excluída logicamente).</param>
public record CategoriaDto(int Id, string Nome, string Tipo, bool Ativo);
