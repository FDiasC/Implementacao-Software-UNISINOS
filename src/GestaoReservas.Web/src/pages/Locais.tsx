import { useMemo, useState } from 'react'
import { Campo, ErroFormulario } from '../components/Campo'
import { Confirmar, Modal } from '../components/Modal'
import { AcoesLinha, AlertaErro, BarraFiltros, BotaoNovo, CabecalhoPagina, EstadoVazio, Selo, SeloAtivo } from '../components/Pagina'
import { Tabela } from '../components/Tabela'
import { useCarregar } from '../hooks/useCarregar'
import { useConfirmacao, useFormulario } from '../hooks/useFormulario'
import { linkPara } from '../hooks/useRota'
import { categoriasApi, locaisApi } from '../services/api'
import type { Categoria, Local } from '../types'
import { contemTexto } from '../utils/formatos'

// Números como texto no formulário para permitir o campo vazio durante a digitação.
const VAZIO = { sala: '', predio: '', capacidade: '', andar: '0', permiteRecursos: false, categoriaId: '' }

export function Locais() {
  const [mostrarInativos, setMostrarInativos] = useState(false)
  const [busca, setBusca] = useState('')
  const [categoriaFiltro, setCategoriaFiltro] = useState('')
  const lista = useCarregar(() => locaisApi.listar(!mostrarInativos), [] as Local[], [mostrarInativos])
  const categorias = useCarregar(
    async () => (await categoriasApi.listar(true)).filter((c) => c.tipo === 'Local'),
    [] as Categoria[],
  )
  const form = useFormulario(VAZIO)
  const confirmacao = useConfirmacao<Local>()

  const visiveis = useMemo(
    () =>
      lista.dados.filter(
        (l) =>
          contemTexto(`${l.predio} ${l.sala} ${l.categoriaNome}`, busca) &&
          (!categoriaFiltro || l.categoriaId === Number(categoriaFiltro)),
      ),
    [lista.dados, busca, categoriaFiltro],
  )

  async function salvar() {
    const { editandoId, valores: v } = form
    const dados = {
      sala: v.sala.trim(),
      predio: v.predio.trim(),
      capacidade: Number(v.capacidade),
      andar: Number(v.andar),
      permiteRecursos: v.permiteRecursos,
      categoriaId: Number(v.categoriaId),
    }
    const ok = await form.enviar(
      () => (editandoId ? locaisApi.atualizar(editandoId, dados) : locaisApi.criar(dados)),
      editandoId ? 'Local atualizado.' : 'Local cadastrado.',
    )
    if (ok) void lista.recarregar()
  }

  function editar(l: Local) {
    form.abrirEdicao(l.id, {
      sala: l.sala,
      predio: l.predio,
      capacidade: String(l.capacidade),
      andar: String(l.andar),
      permiteRecursos: l.permiteRecursos,
      categoriaId: String(l.categoriaId),
    })
  }

  // A categoria de um local em edição pode ter sido desativada: mantém-na na lista para não sumir do select.
  const opcoesCategoria = useMemo(() => {
    const ids = new Set(categorias.dados.map((c) => c.id))
    const atual = lista.dados.find((l) => l.id === form.editandoId)
    return atual && !ids.has(atual.categoriaId)
      ? [...categorias.dados, { id: atual.categoriaId, nome: `${atual.categoriaNome} (inativa)`, tipo: 'Local' as const, ativo: false }]
      : categorias.dados
  }, [categorias.dados, lista.dados, form.editandoId])

  return (
    <section className="pagina">
      <CabecalhoPagina
        titulo="Locais"
        descricao="Salas, auditórios e demais espaços físicos que podem ser reservados."
        acao={<BotaoNovo texto="Novo local" onClick={() => form.abrirNovo()} />}
      />

      <BarraFiltros
        busca={busca}
        aoBuscar={setBusca}
        placeholder="Buscar por prédio, sala ou categoria…"
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
            titulo: 'Local',
            render: (l) => (
              <span className="celula-dupla">
                <strong>Sala {l.sala}</strong>
                <small>{l.predio}</small>
              </span>
            ),
          },
          { titulo: 'Categoria', render: (l) => l.categoriaNome },
          { titulo: 'Andar', render: (l) => (l.andar === 0 ? 'Térreo' : `${l.andar}º`), classe: 'num' },
          { titulo: 'Capacidade', render: (l) => `${l.capacidade} pessoas`, classe: 'num' },
          {
            titulo: 'Recursos',
            render: (l) => (l.permiteRecursos ? <Selo tom="info">Permite</Selo> : <Selo tom="neutro">Não permite</Selo>),
          },
          { titulo: 'Situação', render: (l) => <SeloAtivo ativo={l.ativo} /> },
        ]}
        acoes={(l) => l.ativo && <AcoesLinha aoEditar={() => editar(l)} aoDesativar={() => confirmacao.pedir(l)} />}
        vazio={<EstadoVazio icone="predio" titulo="Nenhum local encontrado" />}
      />

      <Modal
        aberto={form.aberto}
        titulo={form.editandoId ? 'Editar local' : 'Novo local'}
        aoFechar={form.fechar}
        rodape={
          <>
            <button type="button" className="botao botao--secundario" onClick={form.fechar}>
              Cancelar
            </button>
            <button type="submit" form="form-local" className="botao botao--primario" disabled={form.salvando}>
              {form.salvando ? 'Salvando…' : 'Salvar'}
            </button>
          </>
        }
      >
        <form
          id="form-local"
          className="formulario formulario--grade"
          onSubmit={(e) => {
            e.preventDefault()
            void salvar()
          }}
        >
          <Campo rotulo="Prédio" largura="metade">
            <input required maxLength={100} autoFocus value={form.valores.predio} onChange={(e) => form.alterar('predio', e.target.value)} />
          </Campo>
          <Campo rotulo="Sala" largura="metade">
            <input required maxLength={100} value={form.valores.sala} onChange={(e) => form.alterar('sala', e.target.value)} />
          </Campo>
          <Campo rotulo="Capacidade (pessoas)" largura="metade">
            <input type="number" required min={1} value={form.valores.capacidade} onChange={(e) => form.alterar('capacidade', e.target.value)} />
          </Campo>
          <Campo rotulo="Andar" largura="metade">
            <input type="number" required min={0} value={form.valores.andar} onChange={(e) => form.alterar('andar', e.target.value)} />
          </Campo>
          <Campo
            rotulo="Categoria"
            dica={
              categorias.dados.length === 0 && !categorias.carregando ? (
                <>
                  Nenhuma categoria do tipo Local. <a href={linkPara('categorias')}>Cadastre uma</a>.
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
          <label className="caixa-opcao campo--inteira">
            <input
              type="checkbox"
              checked={form.valores.permiteRecursos}
              onChange={(e) => form.alterar('permiteRecursos', e.target.checked)}
            />
            <span>
              <strong>Permite recursos</strong>
              <small>Reservas deste local podem incluir equipamentos (projetor, notebook…).</small>
            </span>
          </label>
          <div className="campo--inteira">
            <ErroFormulario mensagem={form.erro} />
          </div>
        </form>
      </Modal>

      <Confirmar
        aberto={confirmacao.alvo !== null}
        titulo="Desativar local"
        mensagem={
          <>
            Desativar <strong>{confirmacao.alvo && `${confirmacao.alvo.predio} · Sala ${confirmacao.alvo.sala}`}</strong>? Só é possível
            se não houver reservas ativas para ele.
          </>
        }
        textoConfirmar="Desativar"
        processando={confirmacao.processando}
        aoCancelar={confirmacao.cancelar}
        aoConfirmar={async () => {
          if (await confirmacao.executar((l) => locaisApi.desativar(l.id), 'Local desativado.')) void lista.recarregar()
        }}
      />
    </section>
  )
}
