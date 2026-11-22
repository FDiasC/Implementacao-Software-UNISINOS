import { useMemo, useState } from 'react'
import { Campo, ErroFormulario } from '../components/Campo'
import { Confirmar, Modal } from '../components/Modal'
import {
  AcoesLinha,
  AlertaErro,
  BarraFiltros,
  BotaoNovo,
  CabecalhoPagina,
  EstadoVazio,
  Selo,
  SeloAtivo,
} from '../components/Pagina'
import { Tabela } from '../components/Tabela'
import { useCarregar } from '../hooks/useCarregar'
import { useConfirmacao, useFormulario } from '../hooks/useFormulario'
import { categoriasApi } from '../services/api'
import type { Categoria, SalvarCategoria, TipoCategoria } from '../types'
import { contemTexto } from '../utils/formatos'

const VAZIO: SalvarCategoria = { nome: '', tipo: 'Local' }

export function Categorias() {
  const [mostrarInativos, setMostrarInativos] = useState(false)
  const [busca, setBusca] = useState('')
  const [tipo, setTipo] = useState<'' | TipoCategoria>('')
  const lista = useCarregar(() => categoriasApi.listar(!mostrarInativos), [] as Categoria[], [mostrarInativos])
  const form = useFormulario(VAZIO)
  const confirmacao = useConfirmacao<Categoria>()

  const visiveis = useMemo(
    () => lista.dados.filter((c) => contemTexto(c.nome, busca) && (!tipo || c.tipo === tipo)),
    [lista.dados, busca, tipo],
  )

  async function salvar() {
    const { editandoId, valores } = form
    const dados = { ...valores, nome: valores.nome.trim() }
    const ok = await form.enviar(
      () => (editandoId ? categoriasApi.atualizar(editandoId, dados) : categoriasApi.criar(dados)),
      editandoId ? 'Categoria atualizada.' : 'Categoria cadastrada.',
    )
    if (ok) void lista.recarregar()
  }

  return (
    <section className="pagina">
      <CabecalhoPagina
        titulo="Categorias"
        descricao="Classificam locais (ex.: Auditório, Laboratório) e recursos (ex.: Projetor, Notebook)."
        acao={<BotaoNovo texto="Nova categoria" onClick={() => form.abrirNovo()} />}
      />

      <BarraFiltros
        busca={busca}
        aoBuscar={setBusca}
        placeholder="Buscar por nome…"
        mostrarInativos={mostrarInativos}
        aoAlternarInativos={setMostrarInativos}
      >
        <select className="filtro-select" value={tipo} onChange={(e) => setTipo(e.target.value as '' | TipoCategoria)} aria-label="Filtrar por tipo">
          <option value="">Todos os tipos</option>
          <option value="Local">Local</option>
          <option value="Recurso">Recurso</option>
        </select>
      </BarraFiltros>

      {lista.erro && <AlertaErro mensagem={lista.erro} aoTentarNovamente={lista.recarregar} />}

      <Tabela
        itens={visiveis}
        carregando={lista.carregando}
        colunas={[
          { titulo: 'Nome', render: (c) => <strong>{c.nome}</strong> },
          { titulo: 'Tipo', render: (c) => <Selo tom={c.tipo === 'Local' ? 'info' : 'aviso'}>{c.tipo}</Selo> },
          { titulo: 'Situação', render: (c) => <SeloAtivo ativo={c.ativo} /> },
        ]}
        acoes={(c) =>
          c.ativo && (
            <AcoesLinha
              aoEditar={() => form.abrirEdicao(c.id, { nome: c.nome, tipo: c.tipo })}
              aoDesativar={() => confirmacao.pedir(c)}
            />
          )
        }
        vazio={
          <EstadoVazio icone="etiqueta" titulo={busca || tipo ? 'Nenhuma categoria encontrada' : 'Nenhuma categoria cadastrada'}>
            {!busca && !tipo && 'Cadastre categorias antes de criar locais e recursos.'}
          </EstadoVazio>
        }
      />

      <Modal
        aberto={form.aberto}
        titulo={form.editandoId ? 'Editar categoria' : 'Nova categoria'}
        aoFechar={form.fechar}
        rodape={
          <>
            <button type="button" className="botao botao--secundario" onClick={form.fechar}>
              Cancelar
            </button>
            <button type="submit" form="form-categoria" className="botao botao--primario" disabled={form.salvando}>
              {form.salvando ? 'Salvando…' : 'Salvar'}
            </button>
          </>
        }
      >
        <form
          id="form-categoria"
          className="formulario"
          onSubmit={(e) => {
            e.preventDefault()
            void salvar()
          }}
        >
          <Campo rotulo="Nome">
            <input required maxLength={100} autoFocus value={form.valores.nome} onChange={(e) => form.alterar('nome', e.target.value)} />
          </Campo>
          <Campo rotulo="Aplica-se a" dica={form.editandoId ? 'O tipo só muda se nenhum local/recurso usar a categoria.' : undefined}>
            <div className="opcoes-segmentadas">
              {(['Local', 'Recurso'] as const).map((t) => (
                <label key={t} className={`segmento${form.valores.tipo === t ? ' segmento--ativo' : ''}`}>
                  <input type="radio" name="tipo" value={t} checked={form.valores.tipo === t} onChange={() => form.alterar('tipo', t)} />
                  {t === 'Local' ? 'Locais' : 'Recursos'}
                </label>
              ))}
            </div>
          </Campo>
          <ErroFormulario mensagem={form.erro} />
        </form>
      </Modal>

      <Confirmar
        aberto={confirmacao.alvo !== null}
        titulo="Desativar categoria"
        mensagem={
          <>
            Desativar a categoria <strong>{confirmacao.alvo?.nome}</strong>? Ela deixará de aparecer nos cadastros. Só é
            possível se nenhum local ou recurso ativo a utilizar.
          </>
        }
        textoConfirmar="Desativar"
        processando={confirmacao.processando}
        aoCancelar={confirmacao.cancelar}
        aoConfirmar={async () => {
          if (await confirmacao.executar((c) => categoriasApi.desativar(c.id), 'Categoria desativada.')) void lista.recarregar()
        }}
      />
    </section>
  )
}
