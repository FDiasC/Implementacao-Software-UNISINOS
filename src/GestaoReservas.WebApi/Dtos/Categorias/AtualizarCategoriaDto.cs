using System.ComponentModel.DataAnnotations;
using GestaoReservas.Domain.Enums;

namespace GestaoReservas.WebApi.Dtos.Categorias;

/// <summary>Dados para atualizar uma categoria existente.</summary>
/// <param name="Nome">Nome da categoria (até 100 caracteres).</param>
/// <param name="Tipo">Local ou Recurso. Só pode mudar se nenhum local/recurso usar a categoria.</param>
public record AtualizarCategoriaDto(
    [Required, MaxLength(100)] string Nome,
    [EnumDataType(typeof(TipoCategoria))] TipoCategoria Tipo);
