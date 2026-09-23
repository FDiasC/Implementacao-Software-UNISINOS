using GestaoReservas.Domain.Common;
using GestaoReservas.Domain.Enums;

namespace GestaoReservas.Domain.Entities;

public class Categoria : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;

    public TipoCategoria Tipo { get; set; }

    public ICollection<Local> Locais { get; set; } = new List<Local>();

    public ICollection<Recurso> Recursos { get; set; } = new List<Recurso>();
}
