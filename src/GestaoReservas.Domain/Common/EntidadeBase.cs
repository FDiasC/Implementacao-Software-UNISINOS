namespace GestaoReservas.Domain.Common;

public abstract class EntidadeBase
{
    public int Id { get; set; }

    // Exclusão lógica: nenhum registro é apagado do banco; "excluir" é marcar Ativo = false.
    public bool Ativo { get; set; } = true;

    public void Desativar() => Ativo = false;
}
