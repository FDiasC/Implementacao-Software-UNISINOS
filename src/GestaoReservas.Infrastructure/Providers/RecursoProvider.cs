using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;
using GestaoReservas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoReservas.Infrastructure.Providers;

public class RecursoProvider : IRecursoProvider
{
    private readonly AppDbContext _context;
    public RecursoProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Recurso>> ListarAsync(bool apenasAtivos, CancellationToken ct)
    {
        IQueryable<Recurso> query = _context.Recursos.AsNoTracking().Include(r => r.Categoria);

        if (apenasAtivos)
        {
            query = query.Where(r => r.Ativo);
        }

        return await query.OrderBy(r => r.NumeroPatrimonio).ToListAsync(ct);
    }

    public Task<Recurso?> ObterPorIdAsync(int id, CancellationToken ct) =>
        _context.Recursos.Include(r => r.Categoria).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> PossuiReservasAtivasAsync(int recursoId, CancellationToken ct) =>
        _context.Reservas.AnyAsync(r => r.ReservaRecursos.Any(rr => rr.RecursoId == recursoId) && r.Ativo, ct);

    public async Task AdicionarAsync(Recurso recurso, CancellationToken ct) =>
        await _context.Recursos.AddAsync(recurso, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}