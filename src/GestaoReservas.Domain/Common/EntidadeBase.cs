namespace GestaoReservas.Domain.Common;

public abstract class EntidadeBase
{
    public int Id { get; set; }

    public bool Ativo { get; set; } = true;

    public void Desativar() => Ativo = false;
}
