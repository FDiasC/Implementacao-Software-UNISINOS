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

    public async Task<List<Recurso>> ListarRecursosAsync(bool apenasAtivos, CancellationToken ct)
    {
        IQueryable<Recurso> query = _context.Recursos.AsNoTracking().Include(r => r.Categoria);

        if (apenasAtivos)
        {
            query = query.Where(r => r.Ativo);
        }

        return await query.OrderBy(r => r.NumeroPatrimonio).ToListAsync(ct);
    }

    public Task<Recurso?> ObterRecursoPorIdAsync(int id, CancellationToken ct) =>
        _context.Recursos.Include(r => r.Categoria).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> PossuiReservasAtivasAsync(int recursoId, CancellationToken ct) =>
        _context.Reservas.AnyAsync(r => r.ReservaRecursos.Any(rr => rr.RecursoId == recursoId) && r.Ativo, ct);

    public Task<bool> ExisteComNumeroPatrimonioAsync(string numeroPatrimonio, int? idParaIgnorar, CancellationToken ct) =>
        _context.Recursos.AnyAsync(r => r.NumeroPatrimonio == numeroPatrimonio && (idParaIgnorar == null || r.Id != idParaIgnorar), ct);

    public async Task AdicionarRecursoAsync(Recurso recurso, CancellationToken ct) =>
        await _context.Recursos.AddAsync(recurso, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}