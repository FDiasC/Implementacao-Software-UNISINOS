using GestaoReservas.WebApi.Common.Exceptions;
using GestaoReservas.Domain.Dtos.Reservas;
using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;

namespace GestaoReservas.WebApi.Handlers;

public class ReservaHandler : IReservaHandler
{
    private readonly IReservaProvider _reservaProvider;
    private readonly IUsuarioProvider _usuarioProvider;
    private readonly ILocalProvider _localProvider;
    private readonly IRecursoProvider _recursoProvider;

    public ReservaHandler(
        IReservaProvider reservaProvider,
        IUsuarioProvider usuarioProvider,
        ILocalProvider localProvider,
        IRecursoProvider recursoProvider)
    {
        _reservaProvider = reservaProvider;
        _usuarioProvider = usuarioProvider;
        _localProvider = localProvider;
        _recursoProvider = recursoProvider;
    }

    public async Task<List<ReservaDto>> ListarAsync(bool apenasAtivos, CancellationToken ct)
    {
        var reservas = await _reservaProvider.ListarReservasAsync(apenasAtivos, ct);
        return reservas.Select(ParaDto).ToList();
    }

    public async Task<ReservaDto> ObterPorIdAsync(int id, CancellationToken ct)
    {
        var reserva = await _reservaProvider.ObterReservaPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Reserva), id);

        return ParaDto(reserva);
    }

    public async Task<ReservaDto> CriarAsync(CriarReservaDto dto, CancellationToken ct)
    {
        var usuario = await _usuarioProvider.ObterUsuarioPorIdAsync(dto.UsuarioId, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Usuario), dto.UsuarioId);

        if (!usuario.Ativo)
        {
            throw new RegraDeNegocioException($"O usuário '{usuario.Nome}' está inativo.");
        }

        var inicio = Combinar(dto.DataInicial, dto.HoraInicial);
        var fim = Combinar(dto.DataFinal, dto.HoraFinal);

        ValidarPeriodo(inicio, fim, validarPassado: true);

        var (local, recursos) = await ValidarEscopoAsync(dto.LocalId, dto.RecursosId, inicio, fim, ct);
        await ValidarConflitosAsync(null, local, recursos, inicio, fim, ct);

        var reserva = new Reserva
        {
            UsuarioId = usuario.Id,
            Usuario = usuario,
            LocalId = local?.Id,
            Local = local,
            DataInicial = dto.DataInicial,
            HoraInicial = dto.HoraInicial,
            DataFinal = dto.DataFinal,
            HoraFinal = dto.HoraFinal
        };

        foreach (var recurso in recursos)
        {
            reserva.ReservaRecursos.Add(new ReservaRecurso { Recurso = recurso, RecursoId = recurso.Id });
        }

        await _reservaProvider.AdicionarReservaAsync(reserva, ct);
        await _reservaProvider.SalvarAlteracoesAsync(ct);

        return ParaDto(reserva);
    }

    public async Task<ReservaDto> AtualizarAsync(int id, AtualizarReservaDto dto, CancellationToken ct)
    {
        var reserva = await _reservaProvider.ObterReservaPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Reserva), id);

        if (!reserva.Ativo)
        {
            throw new RegraDeNegocioException("Uma reserva inativa não pode ser alterada.");
        }

        var inicio = Combinar(dto.DataInicial, dto.HoraInicial);
        var fim = Combinar(dto.DataFinal, dto.HoraFinal);

        var inicioMudou = dto.DataInicial != reserva.DataInicial || dto.HoraInicial != reserva.HoraInicial;
        ValidarPeriodo(inicio, fim, validarPassado: inicioMudou);

        var (local, recursos) = await ValidarEscopoAsync(dto.LocalId, dto.RecursosId, inicio, fim, ct);
        await ValidarConflitosAsync(reserva.Id, local, recursos, inicio, fim, ct);

        reserva.LocalId = local?.Id;
        reserva.Local = local;
        reserva.DataInicial = dto.DataInicial;
        reserva.HoraInicial = dto.HoraInicial;
        reserva.DataFinal = dto.DataFinal;
        reserva.HoraFinal = dto.HoraFinal;

        var novosIds = recursos.Select(r => r.Id).ToHashSet();

        foreach (var vinculo in reserva.ReservaRecursos.Where(rr => !novosIds.Contains(rr.RecursoId)).ToList())
        {
            reserva.ReservaRecursos.Remove(vinculo);
        }

        var atuais = reserva.ReservaRecursos.Select(rr => rr.RecursoId).ToHashSet();
        foreach (var recurso in recursos.Where(r => !atuais.Contains(r.Id)))
        {
            reserva.ReservaRecursos.Add(new ReservaRecurso { ReservaId = reserva.Id, RecursoId = recurso.Id, Recurso = recurso });
        }

        await _reservaProvider.SalvarAlteracoesAsync(ct);

        return ParaDto(reserva);
    }

    public async Task DesativarAsync(int id, CancellationToken ct)
    {
        var reserva = await _reservaProvider.ObterReservaPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Reserva), id);

        reserva.Desativar();

        await _reservaProvider.SalvarAlteracoesAsync(ct);
    }

    private static DateTime Combinar(DateOnly data, TimeOnly hora) => data.ToDateTime(hora);

    private static void ValidarPeriodo(DateTime inicio, DateTime fim, bool validarPassado)
    {
        if (fim <= inicio)
        {
            throw new RegraDeNegocioException("O término da reserva deve ser posterior ao início.");
        }

        if (validarPassado && inicio < DateTime.Now)
        {
            throw new RegraDeNegocioException("O início da reserva não pode estar no passado.");
        }
    }

    // Valida escopo (local e/ou recursos), PermiteRecursos e janela de dias de cada recurso.
    private async Task<(Local? Local, List<Recurso> Recursos)> ValidarEscopoAsync(
        int? localId, IEnumerable<int>? recursosId, DateTime inicio, DateTime fim, CancellationToken ct)
    {
        var idsRecursos = (recursosId ?? []).Distinct().ToList();

        if (localId is null && idsRecursos.Count == 0)
        {
            throw new RegraDeNegocioException("A reserva deve conter ao menos um local ou um recurso.");
        }

        Local? local = null;
        if (localId is not null)
        {
            local = await _localProvider.ObterLocalPorIdAsync(localId.Value, ct)
                ?? throw new EntidadeNaoEncontradaException(nameof(Local), localId.Value);

            if (!local.Ativo)
            {
                throw new RegraDeNegocioException($"O local '{local.Sala}' está inativo.");
            }

            if (idsRecursos.Count > 0 && !local.PermiteRecursos)
            {
                throw new RegraDeNegocioException($"O local '{local.Sala}' não permite reservas com recursos.");
            }
        }

        // Duração em dias arredondada para cima.
        var duracaoDias = (int)Math.Ceiling((fim - inicio).TotalDays);

        var recursos = new List<Recurso>();
        foreach (var idRecurso in idsRecursos)
        {
            var recurso = await _recursoProvider.ObterRecursoPorIdAsync(idRecurso, ct)
                ?? throw new EntidadeNaoEncontradaException(nameof(Recurso), idRecurso);

            if (!recurso.Ativo)
            {
                throw new RegraDeNegocioException($"O recurso '{recurso.NumeroPatrimonio}' está inativo.");
            }

            if (duracaoDias < recurso.DiasMinimosReserva || duracaoDias > recurso.DiasMaximosReserva)
            {
                throw new RegraDeNegocioException(
                    $"O recurso '{recurso.NumeroPatrimonio}' só pode ser reservado por {recurso.DiasMinimosReserva} a {recurso.DiasMaximosReserva} dia(s); a reserva solicitada tem {duracaoDias}.");
            }

            recursos.Add(recurso);
        }

        return (local, recursos);
    }

    // Impede sobreposição de horário no mesmo local ou no mesmo recurso entre reservas ativas.
    private async Task ValidarConflitosAsync(
        int? idParaIgnorar, Local? local, List<Recurso> recursos, DateTime inicio, DateTime fim, CancellationToken ct)
    {
        var candidatas = await _reservaProvider.ListarAtivasNoPeriodoAsync(
            DateOnly.FromDateTime(inicio), DateOnly.FromDateTime(fim), idParaIgnorar, ct);

        var sobrepostas = candidatas
            .Where(r => Combinar(r.DataInicial, r.HoraInicial) < fim && inicio < Combinar(r.DataFinal, r.HoraFinal))
            .ToList();

        if (local is not null && sobrepostas.Any(r => r.LocalId == local.Id))
        {
            throw new ConflitoException($"O local '{local.Sala}' já está reservado no período informado.");
        }

        foreach (var recurso in recursos)
        {
            if (sobrepostas.Any(r => r.ReservaRecursos.Any(rr => rr.RecursoId == recurso.Id)))
            {
                throw new ConflitoException($"O recurso '{recurso.NumeroPatrimonio}' já está reservado no período informado.");
            }
        }
    }

    private static ReservaDto ParaDto(Reserva reserva) => new(
        reserva.Id,
        reserva.UsuarioId,
        reserva.LocalId,
        reserva.ReservaRecursos.Select(rr => rr.RecursoId).OrderBy(id => id).ToList(),
        reserva.DataInicial,
        reserva.HoraInicial,
        reserva.DataFinal,
        reserva.HoraFinal,
        reserva.Ativo);
}
