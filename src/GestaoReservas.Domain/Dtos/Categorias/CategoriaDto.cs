namespace GestaoReservas.Domain.Dtos.Categorias;

/// <summary>Dados de uma categoria retornados pela API.</summary>
public record CategoriaDto(int Id, string Nome, string Tipo, bool Ativo);
