using GestaoReservas.Domain.Common;

namespace GestaoReservas.Domain.Entities;

public class Usuario : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
