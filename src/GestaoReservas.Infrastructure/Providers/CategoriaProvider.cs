using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;
using GestaoReservas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoReservas.Infrastructure.Providers;

public class CategoriaProvider : ICategoriaProvider
{
    private readonly AppDbContext _context;

    public CategoriaProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Categoria>> ListarAsync(bool apenasAtivas, CancellationToken ct)
    {
        var query = _context.Categorias.AsNoTracking();

        if (apenasAtivas)
        {
            query = query.Where(c => c.Ativo);
        }

        return await query.ToListAsync(ct);
    }

    public Task<Categoria?> ObterPorIdAsync(int id, CancellationToken ct) =>
        _context.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> PossuiVinculosAsync(int categoriaId, bool apenasAtivos, CancellationToken ct) =>
        await _context.Locais.AnyAsync(l => l.CategoriaId == categoriaId && (!apenasAtivos || l.Ativo), ct)
        || await _context.Recursos.AnyAsync(r => r.CategoriaId == categoriaId && (!apenasAtivos || r.Ativo), ct);

    public async Task AdicionarAsync(Categoria categoria, CancellationToken ct) =>
        await _context.Categorias.AddAsync(categoria, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
