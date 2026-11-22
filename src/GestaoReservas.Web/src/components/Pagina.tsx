import type { ReactNode } from 'react'
import { Icone, type NomeIcone } from './Icone'

export function CabecalhoPagina({
  titulo,
  descricao,
  acao,
}: {
  titulo: string
  descricao?: string
  acao?: ReactNode
}) {
  return (
    <header className="pagina__cabecalho">
      <div>
        <h1>{titulo}</h1>
        {descricao && <p className="pagina__descricao">{descricao}</p>}
      </div>
      {acao}
    </header>
  )
}

export function BotaoNovo({ texto, onClick }: { texto: string; onClick: () => void }) {
  return (
    <button type="button" className="botao botao--primario" onClick={onClick}>
      <Icone nome="mais" />
      {texto}
    </button>
  )
}

/** Barra acima das tabelas: busca textual, filtros extras e "mostrar inativos". */
export function BarraFiltros({
  busca,
  aoBuscar,
  placeholder = 'Buscar…',
  mostrarInativos,
  aoAlternarInativos,
  rotuloInativos = 'Mostrar inativos',
  children,
}: {
  busca: string
  aoBuscar: (valor: string) => void
  placeholder?: string
  mostrarInativos: boolean
  aoAlternarInativos: (valor: boolean) => void
  rotuloInativos?: string
  children?: ReactNode
}) {
  return (
    <div className="filtros">
      <label className="busca">
        <Icone nome="busca" />
        <input
          type="search"
          value={busca}
          onChange={(e) => aoBuscar(e.target.value)}
          placeholder={placeholder}
          aria-label="Buscar"
        />
      </label>
      {children}
      <label className="interruptor">
        <input type="checkbox" checked={mostrarInativos} onChange={(e) => aoAlternarInativos(e.target.checked)} />
        <span className="interruptor__trilho" aria-hidden="true" />
        {rotuloInativos}
      </label>
    </div>
  )
}

export function Selo({ tom, children }: { tom: 'neutro' | 'sucesso' | 'perigo' | 'aviso' | 'info'; children: ReactNode }) {
  return <span className={`selo selo--${tom}`}>{children}</span>
}

export const SeloAtivo = ({ ativo }: { ativo: boolean }) =>
  ativo ? <Selo tom="sucesso">Ativo</Selo> : <Selo tom="neutro">Inativo</Selo>

export function EstadoVazio({ icone, titulo, children }: { icone: NomeIcone; titulo: string; children?: ReactNode }) {
  return (
    <div className="vazio">
      <span className="vazio__icone">
        <Icone nome={icone} tamanho={28} />
      </span>
      <strong>{titulo}</strong>
      {children && <p>{children}</p>}
    </div>
  )
}

export function AlertaErro({ mensagem, aoTentarNovamente }: { mensagem: string; aoTentarNovamente?: () => void }) {
  return (
    <div className="alerta alerta--erro" role="alert">
      <Icone nome="alerta" />
      <span>{mensagem}</span>
      {aoTentarNovamente && (
        <button type="button" className="botao botao--secundario botao--pequeno" onClick={aoTentarNovamente}>
          Tentar novamente
        </button>
      )}
    </div>
  )
}

/** Botões editar/desativar de cada linha das tabelas. */
export function AcoesLinha({
  aoEditar,
  aoDesativar,
  rotuloDesativar = 'Desativar',
  desabilitado = false,
}: {
  aoEditar?: () => void
  aoDesativar?: () => void
  rotuloDesativar?: string
  desabilitado?: boolean
}) {
  return (
    <div className="acoes-linha">
      {aoEditar && (
        <button
          type="button"
          className="botao botao--icone botao--fantasma"
          onClick={aoEditar}
          disabled={desabilitado}
          title="Editar"
          aria-label="Editar"
        >
          <Icone nome="editar" />
        </button>
      )}
      {aoDesativar && (
        <button
          type="button"
          className="botao botao--icone botao--fantasma botao--perigo-texto"
          onClick={aoDesativar}
          disabled={desabilitado}
          title={rotuloDesativar}
          aria-label={rotuloDesativar}
        >
          <Icone nome="lixeira" />
        </button>
      )}
    </div>
  )
}
