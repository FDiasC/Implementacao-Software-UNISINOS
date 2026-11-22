namespace GestaoReservas.Domain.Dtos.Usuarios;

/// <summary>Dados de um usuário retornados pela API.</summary>
/// <param name="Email">E-mail (único, sempre em minúsculas).</param>
public record UsuarioDto(int Id, string Nome, string Email, bool Ativo);
