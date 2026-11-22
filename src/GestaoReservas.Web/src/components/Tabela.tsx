import type { ReactNode } from 'react'

export interface Coluna<T> {
  titulo: string
  render: (item: T) => ReactNode
  classe?: string
}

interface TabelaProps<T extends { id: number; ativo: boolean }> {
  colunas: Coluna<T>[]
  itens: T[]
  acoes?: (item: T) => ReactNode
  carregando?: boolean
  vazio: ReactNode
}

/** Tabela das listagens; linhas inativas ficam esmaecidas. Em telas estreitas vira cartões (ver CSS). */
export function Tabela<T extends { id: number; ativo: boolean }>({
  colunas,
  itens,
  acoes,
  carregando = false,
  vazio,
}: TabelaProps<T>) {
  if (carregando && itens.length === 0) {
    return (
      <div className="cartao">
        <div className="carregando" role="status">
          <span className="spinner" aria-hidden="true" />
          Carregando…
        </div>
      </div>
    )
  }

  if (itens.length === 0) return <div className="cartao">{vazio}</div>

  return (
    <div className={`cartao cartao--tabela${carregando ? ' cartao--atualizando' : ''}`}>
      <table className="tabela">
        <thead>
          <tr>
            {colunas.map((c) => (
              <th key={c.titulo} className={c.classe}>
                {c.titulo}
              </th>
            ))}
            {acoes && (
              <th className="tabela__acoes">
                <span className="sr-only">Ações</span>
              </th>
            )}
          </tr>
        </thead>
        <tbody>
          {itens.map((item) => (
            <tr key={item.id} className={item.ativo ? undefined : 'inativo'}>
              {colunas.map((c) => (
                <td key={c.titulo} className={c.classe} data-titulo={c.titulo}>
                  {c.render(item)}
                </td>
              ))}
              {acoes && <td className="tabela__acoes">{acoes(item)}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
