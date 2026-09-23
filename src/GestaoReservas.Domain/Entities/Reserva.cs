using GestaoReservas.Domain.Common;

namespace GestaoReservas.Domain.Entities;

public class Reserva : EntidadeBase
{
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int? LocalId { get; set; }

    public Local? Local { get; set; }

    public DateOnly DataInicial { get; set; }

    public TimeOnly HoraInicial { get; set; }

    public DateOnly DataFinal { get; set; }

    public TimeOnly HoraFinal { get; set; }

    public ICollection<ReservaRecurso> ReservaRecursos { get; set; } = new List<ReservaRecurso>();

    public bool TemEscopoValido => LocalId is not null || ReservaRecursos.Count > 0;
}
