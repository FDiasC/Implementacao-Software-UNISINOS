import { useMemo, useState } from 'react'
import { Campo, ErroFormulario } from '../components/Campo'
import { Icone } from '../components/Icone'
import { Confirmar, Modal } from '../components/Modal'
import { AcoesLinha, AlertaErro, BarraFiltros, BotaoNovo, CabecalhoPagina, EstadoVazio, Selo } from '../components/Pagina'
import { Tabela } from '../components/Tabela'
import { useAuth } from '../contexts/auth'
import { useCarregar } from '../hooks/useCarregar'
import { useConfirmacao, useFormulario } from '../hooks/useFormulario'
import { locaisApi, recursosApi, reservasApi, usuariosApi } from '../services/api'
import type { Local, Recurso, Reserva, Usuario } from '../types'
import {
  ROTULO_SITUACAO,
  contemTexto,
  duracaoEmDias,
  fimDaReserva,
  formatarData,
  formatarHora,
  inicioDaReserva,
  nomeLocal,
  paraHoraApi,
  situacaoDaReserva,
  type SituacaoReserva,
} from '../utils/formatos'

interface FormReserva {
  localId: string
  dataInicial: string
  horaInicial: string
  dataFinal: string
  horaFinal: string
  recursosId: number[]
}

const hoje = () => {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

const VAZIO: FormReserva = { localId: '', dataInicial: '', horaInicial: '', dataFinal: '', horaFinal: '', recursosId: [] }

const TOM_SITUACAO: Record<SituacaoReserva, 'info' | 'sucesso' | 'neutro' | 'perigo'> = {
  agendada: 'info',
  andamento: 'sucesso',
  encerrada: 'neutro',
  cancelada: 'perigo',
}

interface Cadastros {
  usuarios: Usuario[]
  locais: Local[]
  recursos: Recurso[]
}

export function Reservas() {
  const { usuario: logado } = useAuth()
  const [mostrarCanceladas, setMostrarCanceladas] = useState(false)
  const [busca, setBusca] = useState('')
  const [situacaoFiltro, setSituacaoFiltro] = useState<'' | SituacaoReserva>('')
  const [somenteMinhas, setSomenteMinhas] = useState(false)

  const lista = useCarregar(() => reservasApi.listar(!mostrarCanceladas), [] as Reserva[], [mostrarCanceladas])
  // Inclui inativos para conseguir exibir nomes de reservas antigas.
  const cadastros = useCarregar<Cadastros>(
    async () => {
      const [usuarios, locais, recursos] = await Promise.all([
        usuariosApi.listar(false),
        locaisApi.listar(false),
        recursosApi.listar(false),
      ])
      return { usuarios, locais, recursos }
    },
    { usuarios: [], locais: [], recursos: [] },
  )
  // Ativas, para checar conflito de horário no formulário mesmo quando a lista mostra canceladas.
  const ativas = useMemo(() => lista.dados.filter((r) => r.ativo), [lista.dados])

  const form = useFormulario(VAZIO)
  const confirmacao = useConfirmacao<Reserva>()

  const { usuarios, locais, recursos } = cadastros.dados
  const porId = useMemo(
    () => ({
      usuario: new Map(usuarios.map((u) => [u.id, u])),
      local: new Map(locais.map((l) => [l.id, l])),
      recurso: new Map(recursos.map((r) => [r.id, r])),
    }),
    [usuarios, locais, recursos],
  )

  const textoLocal = (id: number | null) => {
    if (id === null) return null
    const l = porId.local.get(id)
    return l ? nomeLocal(l) : `Local #${id}`
  }
  const textoRecurso = (id: number) => porId.recurso.get(id)?.descricao ?? `Recurso #${id}`

  const visiveis = useMemo(() => {
    const agora = new Date()
    return lista.dados.filter((r) => {
      if (somenteMinhas && r.usuarioId !== logado?.id) return false
      if (situacaoFiltro && situacaoDaReserva(r, agora) !== situacaoFiltro) return false
      if (!busca) return true
      const texto = [
        textoLocal(r.localId) ?? '',
        ...r.recursosId.map(textoRecurso),
        porId.usuario.get(r.usuarioId)?.nome ?? '',
      ].join(' ')
      return contemTexto(texto, busca)
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lista.dados, somenteMinhas, situacaoFiltro, busca, porId, logado])

  // ---- Formulário ----
  const v = form.valores
  const localSelecionado = v.localId ? porId.local.get(Number(v.localId)) : undefined
  const recursosBloqueados = !!localSelecionado && !localSelecionado.permiteRecursos
  const dias = duracaoEmDias(v.dataInicial, v.horaInicial, v.dataFinal, v.horaFinal)
  const periodoInvalido =
    !!(v.dataInicial && v.horaInicial && v.dataFinal && v.horaFinal) && dias === null

  // Conflitos de horário com outras reservas ativas (a API faz a checagem definitiva e responde 409).
  const ocupados = useMemo(() => {
    const locaisOcupados = new Set<number>()
    const recursosOcupados = new Set<number>()
    if (dias === null) return { locaisOcupados, recursosOcupados }
    const inicio = new Date(`${v.dataInicial}T${paraHoraApi(v.horaInicial)}`)
    const fim = new Date(`${v.dataFinal}T${paraHoraApi(v.horaFinal)}`)
    for (const r of ativas) {
      if (r.id === form.editandoId) continue
      // Intervalos que só se encostam não conflitam.
      if (inicioDaReserva(r) < fim && inicio < fimDaReserva(r)) {
        if (r.localId !== null) locaisOcupados.add(r.localId)
        r.recursosId.forEach((id) => recursosOcupados.add(id))
      }
    }
    return { locaisOcupados, recursosOcupados }
  }, [ativas, dias, v.dataInicial, v.horaInicial, v.dataFinal, v.horaFinal, form.editandoId])

  // Ativos + os já vinculados à reserva em edição (mesmo se desativados depois).
  const reservaEditada = lista.dados.find((r) => r.id === form.editandoId)
  const locaisDisponiveis = locais.filter((l) => l.ativo || l.id === reservaEditada?.localId)
  const recursosDisponiveis = recursos.filter((r) => r.ativo || reservaEditada?.recursosId.includes(r.id))

  function alterarLocal(localId: string) {
    const local = localId ? porId.local.get(Number(localId)) : undefined
    form.setValores((atual) => ({
      ...atual,
      localId,
      // Local que não permite recursos: limpa a seleção.
      recursosId: local && !local.permiteRecursos ? [] : atual.recursosId,
    }))
  }

  function alternarRecurso(id: number) {
    form.setValores((atual) => ({
      ...atual,
      recursosId: atual.recursosId.includes(id) ? atual.recursosId.filter((r) => r !== id) : [...atual.recursosId, id],
    }))
  }

  function novo() {
    form.abrirNovo({ dataInicial: hoje(), dataFinal: hoje() })
  }

  function editar(r: Reserva) {
    form.abrirEdicao(r.id, {
      localId: r.localId === null ? '' : String(r.localId),
      dataInicial: r.dataInicial,
      horaInicial: formatarHora(r.horaInicial),
      dataFinal: r.dataFinal,
      horaFinal: formatarHora(r.horaFinal),
      recursosId: [...r.recursosId],
    })
  }

  async function salvar() {
    const { editandoId } = form
    const dados = {
      localId: v.localId ? Number(v.localId) : null,
      dataInicial: v.dataInicial,
      horaInicial: paraHoraApi(v.horaInicial),
      dataFinal: v.dataFinal,
      horaFinal: paraHoraApi(v.horaFinal),
      recursosId: v.recursosId,
    }
    const ok = await form.enviar(
      () =>
        editandoId
          ? reservasApi.atualizar(editandoId, dados)
          : reservasApi.criar({ ...dados, usuarioId: logado!.id }),
      editandoId ? 'Reserva atualizada.' : 'Reserva confirmada.',
    )
    if (ok) void lista.recarregar()
  }

  const semEscopo = !v.localId && v.recursosId.length === 0
  const erroCarga = lista.erro ?? cadastros.erro

  return (
    <section className="pagina">
      <CabecalhoPagina
        titulo="Reservas"
        descricao="Reserve um local, recursos ou ambos. Choques de horário são bloqueados automaticamente."
        acao={<BotaoNovo texto="Nova reserva" onClick={novo} />}
      />

      <BarraFiltros
        busca={busca}
        aoBuscar={setBusca}
        placeholder="Buscar por local, recurso ou pessoa…"
        mostrarInativos={mostrarCanceladas}
        aoAlternarInativos={setMostrarCanceladas}
        rotuloInativos="Mostrar canceladas"
      >
        <select
          className="filtro-select"
          value={situacaoFiltro}
          onChange={(e) => setSituacaoFiltro(e.target.value as '' | SituacaoReserva)}
          aria-label="Filtrar por situação"
        >
          <option value="">Todas as situações</option>
          <option value="agendada">Agendadas</option>
          <option value="andamento">Em andamento</option>
          <option value="encerrada">Encerradas</option>
          {mostrarCanceladas && <option value="cancelada">Canceladas</option>}
        </select>
        <label className="interruptor">
          <input type="checkbox" checked={somenteMinhas} onChange={(e) => setSomenteMinhas(e.target.checked)} />
          <span className="interruptor__trilho" aria-hidden="true" />
          Somente minhas
        </label>
      </BarraFiltros>

      {erroCarga && (
        <AlertaErro
          mensagem={erroCarga}
          aoTentarNovamente={() => {
            void lista.recarregar()
            void cadastros.recarregar()
          }}
        />
      )}

      <Tabela
        itens={visiveis}
        carregando={lista.carregando || cadastros.carregando}
        colunas={[
          {
            titulo: 'Período',
            render: (r) => (
              <span className="celula-dupla">
                <strong>
                  {formatarData(r.dataInicial)} {formatarHora(r.horaInicial)}
                </strong>
                <small>
                  até {r.dataFinal === r.dataInicial ? '' : `${formatarData(r.dataFinal)} `}
                  {formatarHora(r.horaFinal)}
                </small>
              </span>
            ),
          },
          {
            titulo: 'Local',
            render: (r) => textoLocal(r.localId) ?? <span className="texto-suave">Sem local</span>,
          },
          {
            titulo: 'Recursos',
            render: (r) =>
              r.recursosId.length === 0 ? (
                <span className="texto-suave">—</span>
              ) : (
                <span className="chips">
                  {r.recursosId.map((id) => (
                    <span key={id} className="chip">
                      {textoRecurso(id)}
                    </span>
                  ))}
                </span>
              ),
          },
          {
            titulo: 'Reservado por',
            render: (r) => (r.usuarioId === logado?.id ? 'Você' : porId.usuario.get(r.usuarioId)?.nome ?? `#${r.usuarioId}`),
          },
          {
            titulo: 'Situação',
            render: (r) => {
              const s = situacaoDaReserva(r)
              return <Selo tom={TOM_SITUACAO[s]}>{ROTULO_SITUACAO[s]}</Selo>
            },
          },
        ]}
        acoes={(r) => {
          if (!r.ativo) return null
          const encerrada = situacaoDaReserva(r) === 'encerrada'
          return (
            <AcoesLinha
              aoEditar={encerrada ? undefined : () => editar(r)}
              aoDesativar={() => confirmacao.pedir(r)}
              rotuloDesativar="Cancelar reserva"
            />
          )
        }}
        vazio={
          <EstadoVazio icone="calendario" titulo={busca || situacaoFiltro || somenteMinhas ? 'Nenhuma reserva encontrada' : 'Nenhuma reserva ainda'}>
            {!busca && !situacaoFiltro && !somenteMinhas && 'Clique em “Nova reserva” para reservar um local ou recurso.'}
          </EstadoVazio>
        }
      />

      <Modal
        aberto={form.aberto}
        largura="larga"
        titulo={form.editandoId ? 'Editar reserva' : 'Nova reserva'}
        aoFechar={form.fechar}
        rodape={
          <>
            <button type="button" className="botao botao--secundario" onClick={form.fechar}>
              Cancelar
            </button>
            <button
              type="submit"
              form="form-reserva"
              className="botao botao--primario"
              disabled={form.salvando || semEscopo || periodoInvalido}
              title={semEscopo ? 'Escolha um local e/ou ao menos um recurso.' : undefined}
            >
              {form.salvando ? 'Salvando…' : form.editandoId ? 'Salvar alterações' : 'Confirmar reserva'}
            </button>
          </>
        }
      >
        <form
          id="form-reserva"
          className="formulario formulario--grade"
          onSubmit={(e) => {
            e.preventDefault()
            void salvar()
          }}
        >
          {form.editandoId && reservaEditada && (
            <p className="campo--inteira texto-suave texto-pequeno">
              Reservado por <strong>{porId.usuario.get(reservaEditada.usuarioId)?.nome ?? `#${reservaEditada.usuarioId}`}</strong> — o
              responsável não muda na edição.
            </p>
          )}

          <fieldset className="grupo campo--inteira">
            <legend>Período</legend>
            <div className="formulario--grade">
              <Campo rotulo="Data de início" largura="metade">
                <input
                  type="date"
                  required
                  min={form.editandoId ? undefined : hoje()}
                  value={v.dataInicial}
                  onChange={(e) => {
                    const valor = e.target.value
                    form.setValores((a) => ({ ...a, dataInicial: valor, dataFinal: a.dataFinal < valor ? valor : a.dataFinal }))
                  }}
                />
              </Campo>
              <Campo rotulo="Hora de início" largura="metade">
                <input type="time" required value={v.horaInicial} onChange={(e) => form.alterar('horaInicial', e.target.value)} />
              </Campo>
              <Campo rotulo="Data de término" largura="metade">
                <input type="date" required min={v.dataInicial || undefined} value={v.dataFinal} onChange={(e) => form.alterar('dataFinal', e.target.value)} />
              </Campo>
              <Campo rotulo="Hora de término" largura="metade">
                <input type="time" required value={v.horaFinal} onChange={(e) => form.alterar('horaFinal', e.target.value)} />
              </Campo>
            </div>
            <p className={`resumo-periodo${periodoInvalido ? ' resumo-periodo--erro' : ''}`}>
              <Icone nome={periodoInvalido ? 'alerta' : 'relogio'} />
              {periodoInvalido
                ? 'O término deve ser posterior ao início.'
                : dias === null
                  ? 'Informe início e término.'
                  : `Duração considerada: ${dias} ${dias === 1 ? 'dia' : 'dias'}`}
            </p>
          </fieldset>

          <Campo rotulo="Local" dica="Opcional quando a reserva tiver ao menos um recurso.">
            <select value={v.localId} onChange={(e) => alterarLocal(e.target.value)}>
              <option value="">Sem local (apenas recursos)</option>
              {locaisDisponiveis.map((l) => (
                <option key={l.id} value={l.id}>
                  {nomeLocal(l)} — {l.capacidade} pessoas{l.permiteRecursos ? '' : ' · sem recursos'}
                  {ocupados.locaisOcupados.has(l.id) ? ' · OCUPADO no período' : ''}
                  {l.ativo ? '' : ' (inativo)'}
                </option>
              ))}
            </select>
          </Campo>
          {localSelecionado && ocupados.locaisOcupados.has(localSelecionado.id) && (
            <div className="alerta alerta--aviso campo--inteira">
              <Icone nome="alerta" />
              Este local já tem outra reserva ativa que se sobrepõe ao período escolhido.
            </div>
          )}

          <fieldset className="grupo campo--inteira" disabled={recursosBloqueados}>
            <legend>Recursos {v.recursosId.length > 0 && <span className="contador">{v.recursosId.length}</span>}</legend>
            {recursosBloqueados ? (
              <p className="texto-suave texto-pequeno">O local escolhido não permite recursos.</p>
            ) : recursosDisponiveis.length === 0 ? (
              <p className="texto-suave texto-pequeno">Nenhum recurso cadastrado.</p>
            ) : (
              <ul className="lista-recursos">
                {recursosDisponiveis.map((r) => {
                  const marcado = v.recursosId.includes(r.id)
                  const foraDaJanela = dias !== null && (dias < r.diasMinimosReserva || dias > r.diasMaximosReserva)
                  const ocupado = ocupados.recursosOcupados.has(r.id)
                  return (
                    <li key={r.id}>
                      <label className={`opcao-recurso${marcado ? ' opcao-recurso--marcado' : ''}`}>
                        <input type="checkbox" checked={marcado} onChange={() => alternarRecurso(r.id)} />
                        <span className="opcao-recurso__texto">
                          <strong>{r.descricao}</strong>
                          <small>
                            <span className="mono">{r.numeroPatrimonio}</span> · {r.categoriaNome} ·{' '}
                            {r.diasMinimosReserva === r.diasMaximosReserva
                              ? `${r.diasMinimosReserva} dia(s)`
                              : `${r.diasMinimosReserva}–${r.diasMaximosReserva} dias`}
                          </small>
                        </span>
                        {ocupado && <Selo tom="perigo">Ocupado</Selo>}
                        {!ocupado && foraDaJanela && <Selo tom="aviso">Fora da duração</Selo>}
                      </label>
                    </li>
                  )
                })}
              </ul>
            )}
          </fieldset>

          {semEscopo && <p className="campo--inteira texto-suave texto-pequeno">Escolha um local e/ou ao menos um recurso.</p>}

          <div className="campo--inteira">
            <ErroFormulario mensagem={form.erro} />
          </div>
        </form>
      </Modal>

      <Confirmar
        aberto={confirmacao.alvo !== null}
        titulo="Cancelar reserva"
        mensagem={
          confirmacao.alvo && (
            <>
              Cancelar a reserva de{' '}
              <strong>
                {formatarData(confirmacao.alvo.dataInicial)} {formatarHora(confirmacao.alvo.horaInicial)}
              </strong>
              {confirmacao.alvo.localId !== null && <> em {textoLocal(confirmacao.alvo.localId)}</>}? O local e os recursos ficam livres
              para outras reservas. A reserva continua no histórico como cancelada.
            </>
          )
        }
        textoConfirmar="Cancelar reserva"
        processando={confirmacao.processando}
        aoCancelar={confirmacao.cancelar}
        aoConfirmar={async () => {
          if (await confirmacao.executar((r) => reservasApi.desativar(r.id), 'Reserva cancelada.')) void lista.recarregar()
        }}
      />
    </section>
  )
}
