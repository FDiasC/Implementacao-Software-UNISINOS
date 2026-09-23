namespace GestaoReservas.Domain.Entities;

public class ReservaRecurso
{
    public int ReservaId { get; set; }

    public Reserva Reserva { get; set; } = null!;

    public int RecursoId { get; set; }

    public Recurso Recurso { get; set; } = null!;
}
