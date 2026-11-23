import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Tabela, type Coluna } from './Tabela'

interface Item {
  id: number
  nome: string
  ativo: boolean
}

const colunas: Coluna<Item>[] = [{ titulo: 'Nome', render: (i) => i.nome }]

describe('Tabela', () => {
  it('mostra o estado vazio quando não há itens', () => {
    render(<Tabela colunas={colunas} itens={[]} vazio={<p>Nada por aqui</p>} />)
    expect(screen.getByText('Nada por aqui')).toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('mostra "Carregando…" na primeira carga', () => {
    render(<Tabela colunas={colunas} itens={[]} carregando vazio={<p>vazio</p>} />)
    expect(screen.getByRole('status')).toHaveTextContent('Carregando…')
  })

  it('renderiza uma linha por item e esmaece as inativas', () => {
    const itens = [
      { id: 1, nome: 'Sala 101', ativo: true },
      { id: 2, nome: 'Sala 202', ativo: false },
    ]
    render(<Tabela colunas={colunas} itens={itens} vazio={null} />)

    const linhas = screen.getAllByRole('row').slice(1)
    expect(linhas).toHaveLength(2)
    expect(linhas[0]).not.toHaveClass('inativo')
    expect(linhas[1]).toHaveClass('inativo')
  })

  it('marca cada célula com o título da coluna (usado no layout em cartões do celular)', () => {
    render(<Tabela colunas={colunas} itens={[{ id: 1, nome: 'Sala 101', ativo: true }]} vazio={null} />)
    expect(screen.getByText('Sala 101')).toHaveAttribute('data-titulo', 'Nome')
  })

  it('inclui a coluna de ações quando informada', () => {
    render(
      <Tabela
        colunas={colunas}
        itens={[{ id: 1, nome: 'Sala 101', ativo: true }]}
        acoes={(i) => <button type="button">Editar {i.nome}</button>}
        vazio={null}
      />,
    )
    const linha = screen.getAllByRole('row')[1]
    expect(within(linha).getByRole('button', { name: 'Editar Sala 101' })).toBeInTheDocument()
  })
})
