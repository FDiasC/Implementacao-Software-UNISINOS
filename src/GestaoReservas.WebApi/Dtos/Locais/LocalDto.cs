namespace GestaoReservas.WebApi.Dtos.Locais;

/// <summary>Dados de um local retornados pela API.</summary>
/// <param name="Id">Identificador do local.</param>
/// <param name="Sala">Identificação da sala.</param>
/// <param name="Predio">Prédio onde a sala fica.</param>
/// <param name="Capacidade">Número máximo de pessoas.</param>
/// <param name="Andar">Andar do prédio.</param>
/// <param name="PermiteRecursos">Se reservas deste local podem incluir recursos.</param>
/// <param name="CategoriaId">Identificador da categoria do local.</param>
/// <param name="CategoriaNome">Nome da categoria do local.</param>
/// <param name="Ativo">Indica se o local está ativo (false = excluído logicamente).</param>
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
