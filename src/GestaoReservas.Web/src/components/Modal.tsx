import { useEffect, useRef, type ReactNode } from 'react'
import { Icone } from './Icone'

interface ModalProps {
  aberto: boolean
  titulo: string
  aoFechar: () => void
  children: ReactNode
  /** Botões do rodapé. */
  rodape?: ReactNode
  largura?: 'normal' | 'larga'
}

/** Janela modal baseada em <dialog> nativo (foco preso e Esc para fechar já vêm do navegador). */
export function Modal({ aberto, titulo, aoFechar, children, rodape, largura = 'normal' }: ModalProps) {
  const ref = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (aberto && !dialog.open) {
      dialog.showModal()
      // showModal() foca o primeiro elemento focável (o botão fechar); leva o foco ao primeiro campo.
      dialog.querySelector<HTMLElement>('.modal__corpo :is(input, select, textarea):not([disabled])')?.focus()
    }
    if (!aberto && dialog.open) dialog.close()
  }, [aberto])

  return (
    <dialog
      ref={ref}
      className={`modal modal--${largura}`}
      onCancel={(e) => {
        e.preventDefault()
        aoFechar()
      }}
      onClick={(e) => {
        // Clique no fundo (fora da caixa) fecha.
        if (e.target === e.currentTarget) aoFechar()
      }}
    >
      {aberto && (
        <div className="modal__caixa">
          <header className="modal__cabecalho">
            <h2>{titulo}</h2>
            <button type="button" className="botao botao--icone botao--fantasma" onClick={aoFechar} aria-label="Fechar">
              <Icone nome="x" />
            </button>
          </header>
          <div className="modal__corpo">{children}</div>
          {rodape && <footer className="modal__rodape">{rodape}</footer>}
        </div>
      )}
    </dialog>
  )
}

interface ConfirmarProps {
  aberto: boolean
  titulo: string
  mensagem: ReactNode
  textoConfirmar?: string
  processando?: boolean
  aoConfirmar: () => void
  aoCancelar: () => void
}

/** Confirmação de ação destrutiva (desativar/cancelar). */
export function Confirmar({
  aberto,
  titulo,
  mensagem,
  textoConfirmar = 'Confirmar',
  processando = false,
  aoConfirmar,
  aoCancelar,
}: ConfirmarProps) {
  return (
    <Modal
      aberto={aberto}
      titulo={titulo}
      aoFechar={aoCancelar}
      rodape={
        <>
          <button type="button" className="botao botao--secundario" onClick={aoCancelar} disabled={processando}>
            Voltar
          </button>
          <button type="button" className="botao botao--perigo" onClick={aoConfirmar} disabled={processando}>
            {processando ? 'Processando…' : textoConfirmar}
          </button>
        </>
      }
    >
      <p className="texto-confirmacao">{mensagem}</p>
    </Modal>
  )
}
