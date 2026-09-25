using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;
using GestaoReservas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoReservas.Infrastructure.Providers;

public class UsuarioProvider : IUsuarioProvider
{
    private readonly AppDbContext _context;

    public UsuarioProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> ListarAsync(bool apenasAtivos, CancellationToken ct)
    {
        var query = _context.Usuarios.AsNoTracking();

        if (apenasAtivos)
        {
            query = query.Where(u => u.Ativo);
        }

        return await query.OrderBy(u => u.Id).ToListAsync(ct);
    }

    public Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct) =>
        _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> ExisteComEmailAsync(string email, int? idParaIgnorar, CancellationToken ct) =>
        _context.Usuarios.AnyAsync(u => u.Email.ToLower() == email.ToLower() && (idParaIgnorar == null || u.Id != idParaIgnorar), ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct) =>
        await _context.Usuarios.AddAsync(usuario, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
