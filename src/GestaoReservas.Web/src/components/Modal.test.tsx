import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { Confirmar, Modal } from './Modal'

describe('Modal', () => {
  it('não mostra o conteúdo enquanto estiver fechado', () => {
    render(
      <Modal aberto={false} titulo="Novo local" aoFechar={() => {}}>
        <input aria-label="Prédio" />
      </Modal>,
    )
    expect(screen.queryByText('Novo local')).not.toBeInTheDocument()
  })

  it('ao abrir, coloca o foco no primeiro campo do formulário', () => {
    render(
      <Modal aberto titulo="Novo local" aoFechar={() => {}}>
        <input aria-label="Prédio" />
        <input aria-label="Sala" />
      </Modal>,
    )
    expect(screen.getByLabelText('Prédio')).toHaveFocus()
  })

  it('fecha pelo botão X e pela tecla Esc', async () => {
    const aoFechar = vi.fn()
    render(
      <Modal aberto titulo="Novo local" aoFechar={aoFechar}>
        <p>conteúdo</p>
      </Modal>,
    )

    await userEvent.click(screen.getByRole('button', { name: 'Fechar' }))
    // O navegador dispara "cancel" no <dialog> quando a pessoa aperta Esc.
    fireEvent(screen.getByRole('dialog'), new Event('cancel', { cancelable: true }))

    expect(aoFechar).toHaveBeenCalledTimes(2)
  })
})

describe('Confirmar', () => {
  it('chama aoConfirmar ou aoCancelar conforme o botão', async () => {
    const aoConfirmar = vi.fn()
    const aoCancelar = vi.fn()
    render(
      <Confirmar
        aberto
        titulo="Cancelar reserva"
        mensagem="Cancelar a reserva de 25/11?"
        textoConfirmar="Cancelar reserva"
        aoConfirmar={aoConfirmar}
        aoCancelar={aoCancelar}
      />,
    )

    expect(screen.getByText('Cancelar a reserva de 25/11?')).toBeInTheDocument()
    await userEvent.click(screen.getByRole('button', { name: 'Cancelar reserva' }))
    await userEvent.click(screen.getByRole('button', { name: 'Voltar' }))

    expect(aoConfirmar).toHaveBeenCalledOnce()
    expect(aoCancelar).toHaveBeenCalledOnce()
  })

  it('bloqueia os botões enquanto a ação está em andamento', () => {
    render(<Confirmar aberto processando titulo="Desativar" mensagem="?" aoConfirmar={() => {}} aoCancelar={() => {}} />)

    expect(screen.getByRole('button', { name: 'Processando…' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Voltar' })).toBeDisabled()
  })
})
