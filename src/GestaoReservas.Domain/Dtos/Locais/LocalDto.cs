namespace GestaoReservas.Domain.Dtos.Locais;

/// <summary>Dados de um local retornados pela API.</summary>
public record LocalDto(
    int Id,
    string Sala,
    string Predio,
    int Capacidade,
    int Andar,
    bool PermiteRecursos,
    int CategoriaId,
    string CategoriaNome,
    bool Ativo);
