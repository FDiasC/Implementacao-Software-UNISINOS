import { useMemo } from 'react'
import { Icone, type NomeIcone } from '../components/Icone'
import { AlertaErro, EstadoVazio, Selo } from '../components/Pagina'
import { useAuth } from '../contexts/auth'
import { useCarregar } from '../hooks/useCarregar'
import { linkPara, type Rota } from '../hooks/useRota'
import { locaisApi, recursosApi, reservasApi } from '../services/api'
import type { Local, Recurso, Reserva } from '../types'
import { fimDaReserva, formatarData, formatarHora, inicioDaReserva, nomeLocal, situacaoDaReserva } from '../utils/formatos'

interface Dados {
  reservas: Reserva[]
  locais: Local[]
  recursos: Recurso[]
}

function Indicador({ icone, rotulo, valor, rota }: { icone: NomeIcone; rotulo: string; valor: number | string; rota: Rota }) {
  return (
    <a className="cartao indicador" href={linkPara(rota)}>
      <span className="indicador__icone">
        <Icone nome={icone} />
      </span>
      <span>
        <span className="indicador__valor">{valor}</span>
        <span className="indicador__rotulo">{rotulo}</span>
      </span>
    </a>
  )
}

export function Inicio() {
  const { usuario } = useAuth()
  const { dados, carregando, erro, recarregar } = useCarregar<Dados>(
    async () => {
      const [reservas, locais, recursos] = await Promise.all([
        reservasApi.listar(true),
        locaisApi.listar(false),
        recursosApi.listar(false),
      ])
      return { reservas, locais, recursos }
    },
    { reservas: [], locais: [], recursos: [] },
  )

  const resumo = useMemo(() => {
    const agora = new Date()
    const vigentes = dados.reservas.filter((r) => fimDaReserva(r) > agora)
    const emAndamento = vigentes.filter((r) => inicioDaReserva(r) <= agora)
    const minhas = vigentes.filter((r) => r.usuarioId === usuario?.id)
    const proximas = [...minhas].sort((a, b) => inicioDaReserva(a).getTime() - inicioDaReserva(b).getTime()).slice(0, 5)
    return {
      vigentes: vigentes.length,
      emAndamento: emAndamento.length,
      minhas: minhas.length,
      proximas,
      locaisAtivos: dados.locais.filter((l) => l.ativo).length,
      recursosAtivos: dados.recursos.filter((r) => r.ativo).length,
    }
  }, [dados, usuario])

  const local = (id: number | null) => {
    const l = id === null ? undefined : dados.locais.find((x) => x.id === id)
    return l ? nomeLocal(l) : null
  }
  const recurso = (id: number) => dados.recursos.find((r) => r.id === id)?.descricao ?? `#${id}`
  const primeiroNome = usuario?.nome.split(' ')[0] ?? ''

  return (
    <section className="pagina">
      <header className="pagina__cabecalho">
        <div>
          <h1>Olá, {primeiroNome}</h1>
          <p className="pagina__descricao">Resumo das reservas e dos cadastros.</p>
        </div>
        <a className="botao botao--primario" href={linkPara('reservas')}>
          <Icone nome="calendario" />
          Ir para reservas
        </a>
      </header>

      {erro && <AlertaErro mensagem={erro} aoTentarNovamente={recarregar} />}

      <div className="indicadores">
        <Indicador icone="calendario" rotulo="Reservas vigentes" valor={carregando ? '…' : resumo.vigentes} rota="reservas" />
        <Indicador icone="relogio" rotulo="Em andamento agora" valor={carregando ? '…' : resumo.emAndamento} rota="reservas" />
        <Indicador icone="predio" rotulo="Locais ativos" valor={carregando ? '…' : resumo.locaisAtivos} rota="locais" />
        <Indicador icone="caixa" rotulo="Recursos ativos" valor={carregando ? '…' : resumo.recursosAtivos} rota="recursos" />
      </div>

      <div className="cartao">
        <div className="cartao__cabecalho">
          <h2>Minhas próximas reservas</h2>
          {resumo.minhas > 0 && <Selo tom="info">{resumo.minhas} vigente(s)</Selo>}
        </div>
        {!carregando && resumo.proximas.length === 0 ? (
          <EstadoVazio icone="calendario" titulo="Você não tem reservas futuras">
            <a href={linkPara('reservas')}>Fazer uma reserva</a>
          </EstadoVazio>
        ) : (
          <ul className="agenda">
            {resumo.proximas.map((r) => (
              <li key={r.id} className="agenda__item">
                <span className="agenda__data">
                  <strong>{formatarData(r.dataInicial).slice(0, 5)}</strong>
                  <small>{formatarHora(r.horaInicial)}</small>
                </span>
                <span className="agenda__texto">
                  <strong>{local(r.localId) ?? 'Somente recursos'}</strong>
                  <small>
                    até {r.dataFinal !== r.dataInicial && `${formatarData(r.dataFinal)} `}
                    {formatarHora(r.horaFinal)}
                    {r.recursosId.length > 0 && ` · ${r.recursosId.map(recurso).join(', ')}`}
                  </small>
                </span>
                {situacaoDaReserva(r) === 'andamento' ? <Selo tom="sucesso">Em andamento</Selo> : <Selo tom="info">Agendada</Selo>}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
