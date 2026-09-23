using GestaoReservas.Domain.Common;

namespace GestaoReservas.Domain.Entities;

public class Recurso : EntidadeBase
{
    public string Descricao { get; set; } = string.Empty;

    public string NumeroPatrimonio { get; set; } = string.Empty;

    public int DiasMinimosReserva { get; set; }

    public int DiasMaximosReserva { get; set; }

    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public ICollection<ReservaRecurso> ReservaRecursos { get; set; } = new List<ReservaRecurso>();
}
