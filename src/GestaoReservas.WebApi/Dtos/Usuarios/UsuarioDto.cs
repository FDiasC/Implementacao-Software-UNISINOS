namespace GestaoReservas.WebApi.Dtos.Usuarios;

/// <summary>Dados de um usuário retornados pela API.</summary>
/// <param name="Id">Identificador do usuário.</param>
/// <param name="Nome">Nome completo.</param>
/// <param name="Email">E-mail (único, sempre em minúsculas).</param>
/// <param name="Ativo">Indica se o usuário está ativo (false = excluído logicamente).</param>
public record UsuarioDto(int Id, string Nome, string Email, bool Ativo);
