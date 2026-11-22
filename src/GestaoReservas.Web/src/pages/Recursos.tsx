import { useMemo, useState } from 'react'
import { Campo, ErroFormulario } from '../components/Campo'
import { Confirmar, Modal } from '../components/Modal'
import { AcoesLinha, AlertaErro, BarraFiltros, BotaoNovo, CabecalhoPagina, EstadoVazio, SeloAtivo } from '../components/Pagina'
import { Tabela } from '../components/Tabela'
import { useCarregar } from '../hooks/useCarregar'
import { useConfirmacao, useFormulario } from '../hooks/useFormulario'
import { linkPara } from '../hooks/useRota'
import { categoriasApi, recursosApi } from '../services/api'
import type { Categoria, Recurso } from '../types'
import { contemTexto } from '../utils/formatos'

const VAZIO = { numeroPatrimonio: '', descricao: '', diasMinimosReserva: '1', diasMaximosReserva: '1', categoriaId: '' }

const plural = (n: number) => `${n} ${n === 1 ? 'dia' : 'dias'}`

export function Recursos() {
  const [mostrarInativos, setMostrarInativos] = useState(false)
  const [busca, setBusca] = useState('')
  const [categoriaFiltro, setCategoriaFiltro] = useState('')
  const lista = useCarregar(() => recursosApi.listar(!mostrarInativos), [] as Recurso[], [mostrarInativos])
  const categorias = useCarregar(
    async () => (await categoriasApi.listar(true)).filter((c) => c.tipo === 'Recurso'),
    [] as Categoria[],
  )
  const form = useFormulario(VAZIO)
  const confirmacao = useConfirmacao<Recurso>()

  const visiveis = useMemo(
    () =>
      lista.dados.filter(
        (r) =>
          contemTexto(`${r.descricao} ${r.numeroPatrimonio} ${r.categoriaNome}`, busca) &&
          (!categoriaFiltro || r.categoriaId === Number(categoriaFiltro)),
      ),
    [lista.dados, busca, categoriaFiltro],
  )

  const minimo = Number(form.valores.diasMinimosReserva)
  const maximo = Number(form.valores.diasMaximosReserva)
  const janelaInvalida = form.valores.diasMaximosReserva !== '' && maximo < minimo

  async function salvar() {
    const { editandoId, valores: v } = form
    const dados = {
      numeroPatrimonio: v.numeroPatrimonio.trim(),
      descricao: v.descricao.trim(),
      diasMinimosReserva: Number(v.diasMinimosReserva),
      diasMaximosReserva: Number(v.diasMaximosReserva),
      categoriaId: Number(v.categoriaId),
    }
    const ok = await form.enviar(
      () => (editandoId ? recursosApi.atualizar(editandoId, dados) : recursosApi.criar(dados)),
      editandoId ? 'Recurso atualizado.' : 'Recurso cadastrado.',
    )
    if (ok) void lista.recarregar()
  }

  function editar(r: Recurso) {
    form.abrirEdicao(r.id, {
      numeroPatrimonio: r.numeroPatrimonio,
      descricao: r.descricao,
      diasMinimosReserva: String(r.diasMinimosReserva),
      diasMaximosReserva: String(r.diasMaximosReserva),
      categoriaId: String(r.categoriaId),
    })
  }

  const opcoesCategoria = useMemo(() => {
    const ids = new Set(categorias.dados.map((c) => c.id))
    const atual = lista.dados.find((r) => r.id === form.editandoId)
    return atual && !ids.has(atual.categoriaId)
      ? [...categorias.dados, { id: atual.categoriaId, nome: `${atual.categoriaNome} (inativa)`, tipo: 'Recurso' as const, ativo: false }]
      : categorias.dados
  }, [categorias.dados, lista.dados, form.editandoId])

  return (
    <section className="pagina">
      <CabecalhoPagina
        titulo="Recursos"
        descricao="Equipamentos físicos, identificados pelo número de patrimônio."
        acao={<BotaoNovo texto="Novo recurso" onClick={() => form.abrirNovo()} />}
      />

      <BarraFiltros
        busca={busca}
        aoBuscar={setBusca}
        placeholder="Buscar por descrição, patrimônio ou categoria…"
        mostrarInativos={mostrarInativos}
        aoAlternarInativos={setMostrarInativos}
      >
        <select className="filtro-select" value={categoriaFiltro} onChange={(e) => setCategoriaFiltro(e.target.value)} aria-label="Filtrar por categoria">
          <option value="">Todas as categorias</option>
          {categorias.dados.map((c) => (
            <option key={c.id} value={c.id}>
              {c.nome}
            </option>
          ))}
        </select>
      </BarraFiltros>

      {lista.erro && <AlertaErro mensagem={lista.erro} aoTentarNovamente={lista.recarregar} />}

      <Tabela
        itens={visiveis}
        carregando={lista.carregando}
        colunas={[
          {
            titulo: 'Recurso',
            render: (r) => (
              <span className="celula-dupla">
                <strong>{r.descricao}</strong>
                <small className="mono">Patrimônio {r.numeroPatrimonio}</small>
              </span>
            ),
          },
          { titulo: 'Categoria', render: (r) => r.categoriaNome },
          {
            titulo: 'Duração permitida',
            render: (r) =>
              r.diasMinimosReserva === r.diasMaximosReserva
                ? plural(r.diasMinimosReserva)
                : `${r.diasMinimosReserva} a ${plural(r.diasMaximosReserva)}`,
          },
          { titulo: 'Situação', render: (r) => <SeloAtivo ativo={r.ativo} /> },
        ]}
        acoes={(r) => r.ativo && <AcoesLinha aoEditar={() => editar(r)} aoDesativar={() => confirmacao.pedir(r)} />}
        vazio={<EstadoVazio icone="caixa" titulo="Nenhum recurso encontrado" />}
      />

      <Modal
        aberto={form.aberto}
        titulo={form.editandoId ? 'Editar recurso' : 'Novo recurso'}
        aoFechar={form.fechar}
        rodape={
          <>
            <button type="button" className="botao botao--secundario" onClick={form.fechar}>
              Cancelar
            </button>
            <button type="submit" form="form-recurso" className="botao botao--primario" disabled={form.salvando || janelaInvalida}>
              {form.salvando ? 'Salvando…' : 'Salvar'}
            </button>
          </>
        }
      >
        <form
          id="form-recurso"
          className="formulario formulario--grade"
          onSubmit={(e) => {
            e.preventDefault()
            void salvar()
          }}
        >
          <Campo rotulo="Descrição">
            <input required maxLength={200} autoFocus value={form.valores.descricao} onChange={(e) => form.alterar('descricao', e.target.value)} />
          </Campo>
          <Campo rotulo="Número de patrimônio" largura="metade">
            <input
              required
              maxLength={50}
              className="mono"
              value={form.valores.numeroPatrimonio}
              onChange={(e) => form.alterar('numeroPatrimonio', e.target.value)}
            />
          </Campo>
          <Campo
            rotulo="Categoria"
            largura="metade"
            dica={
              categorias.dados.length === 0 && !categorias.carregando ? (
                <>
                  Nenhuma categoria do tipo Recurso. <a href={linkPara('categorias')}>Cadastre uma</a>.
                </>
              ) : undefined
            }
          >
            <select required value={form.valores.categoriaId} onChange={(e) => form.alterar('categoriaId', e.target.value)}>
              <option value="">Selecione…</option>
              {opcoesCategoria.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nome}
                </option>
              ))}
            </select>
          </Campo>
          <Campo rotulo="Mínimo de dias por reserva" largura="metade">
            <input
              type="number"
              required
              min={1}
              value={form.valores.diasMinimosReserva}
              onChange={(e) => form.alterar('diasMinimosReserva', e.target.value)}
            />
          </Campo>
          <Campo
            rotulo="Máximo de dias por reserva"
            largura="metade"
            dica={janelaInvalida ? <span className="texto-erro">Não pode ser menor que o mínimo.</span> : undefined}
          >
            <input
              type="number"
              required
              min={1}
              value={form.valores.diasMaximosReserva}
              onChange={(e) => form.alterar('diasMaximosReserva', e.target.value)}
            />
          </Campo>
          <p className="campo--inteira texto-suave texto-pequeno">
            A duração da reserva é arredondada para cima: 1 dia e 2 horas conta como 2 dias.
          </p>
          <div className="campo--inteira">
            <ErroFormulario mensagem={form.erro} />
          </div>
        </form>
      </Modal>

      <Confirmar
        aberto={confirmacao.alvo !== null}
        titulo="Desativar recurso"
        mensagem={
          <>
            Desativar <strong>{confirmacao.alvo?.descricao}</strong> (patrimônio {confirmacao.alvo?.numeroPatrimonio})? Só é possível
            se não houver reservas ativas para ele.
          </>
        }
        textoConfirmar="Desativar"
        processando={confirmacao.processando}
        aoCancelar={confirmacao.cancelar}
        aoConfirmar={async () => {
          if (await confirmacao.executar((r) => recursosApi.desativar(r.id), 'Recurso desativado.')) void lista.recarregar()
        }}
      />
    </section>
  )
}
