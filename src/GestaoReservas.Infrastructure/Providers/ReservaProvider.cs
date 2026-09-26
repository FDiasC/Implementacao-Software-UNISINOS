using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;
using GestaoReservas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoReservas.Infrastructure.Providers;

public class ReservaProvider : IReservaProvider
{
    private readonly AppDbContext _context;

    public ReservaProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reserva>> ListarReservasAsync(bool apenasAtivos, CancellationToken ct)
    {
        IQueryable<Reserva> query = _context.Reservas.AsNoTracking().Include(r => r.ReservaRecursos);

        if (apenasAtivos)
        {
            query = query.Where(r => r.Ativo);
        }

        return await query.OrderBy(r => r.DataInicial).ThenBy(r => r.HoraInicial).ThenBy(r => r.Id).ToListAsync(ct);
    }

    public Task<Reserva?> ObterReservaPorIdAsync(int id, CancellationToken ct) =>
        _context.Reservas.Include(r => r.ReservaRecursos).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<Reserva>> ListarAtivasNoPeriodoAsync(DateOnly dataInicial, DateOnly dataFinal, int? idParaIgnorar, CancellationToken ct) =>
        _context.Reservas
            .AsNoTracking()
            .Include(r => r.ReservaRecursos)
            .Where(r => r.Ativo
                && r.DataInicial <= dataFinal
                && r.DataFinal >= dataInicial
                && (idParaIgnorar == null || r.Id != idParaIgnorar))
            .ToListAsync(ct);

    public async Task AdicionarReservaAsync(Reserva reserva, CancellationToken ct) =>
        await _context.Reservas.AddAsync(reserva, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
