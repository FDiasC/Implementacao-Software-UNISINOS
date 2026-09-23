using GestaoReservas.Domain.Common;

namespace GestaoReservas.Domain.Entities;

public class Local : EntidadeBase
{
    public string Sala { get; set; } = string.Empty;

    public string Predio { get; set; } = string.Empty;

    public int Capacidade { get; set; }

    public int Andar { get; set; }

    public bool PermiteRecursos { get; set; }

    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
