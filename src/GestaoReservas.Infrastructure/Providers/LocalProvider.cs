using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;
using GestaoReservas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoReservas.Infrastructure.Providers;

public class LocalProvider : ILocalProvider
{
    private readonly AppDbContext _context;

    public LocalProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Local>> ListarLocaisAsync(bool apenasAtivos, CancellationToken ct)
    {
        IQueryable<Local> query = _context.Locais.AsNoTracking().Include(l => l.Categoria);

        if (apenasAtivos)
        {
            query = query.Where(l => l.Ativo);
        }

        return await query.OrderBy(l => l.Predio).ThenBy(l => l.Sala).ToListAsync(ct);
    }

    public Task<Local?> ObterLocalPorIdAsync(int id, CancellationToken ct) =>
        _context.Locais.Include(l => l.Categoria).FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<bool> PossuiReservasAtivasAsync(int localId, CancellationToken ct) =>
        _context.Reservas.AnyAsync(r => r.LocalId == localId && r.Ativo, ct);

    public async Task AdicionarLocalAsync(Local local, CancellationToken ct) =>
        await _context.Locais.AddAsync(local, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
