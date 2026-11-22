import { useCallback, useMemo, useRef, useState, type ReactNode } from 'react'
import { Icone } from '../components/Icone'
import { NotificacoesContext, type TipoNotificacao } from './notificacoes'

interface Notificacao {
  id: number
  mensagem: string
  tipo: TipoNotificacao
}

const DURACAO_MS = 4500

/** Avisos temporários ("toasts") no canto da tela para sucesso/erro das operações. */
export function NotificacoesProvider({ children }: { children: ReactNode }) {
  const [itens, setItens] = useState<Notificacao[]>([])
  const proximoId = useRef(1)

  const fechar = useCallback((id: number) => {
    setItens((atual) => atual.filter((n) => n.id !== id))
  }, [])

  const notificar = useCallback(
    (mensagem: string, tipo: TipoNotificacao = 'sucesso') => {
      const id = proximoId.current++
      setItens((atual) => [...atual, { id, mensagem, tipo }])
      window.setTimeout(() => fechar(id), tipo === 'erro' ? DURACAO_MS * 2 : DURACAO_MS)
    },
    [fechar],
  )

  const valor = useMemo(() => ({ notificar }), [notificar])

  return (
    <NotificacoesContext.Provider value={valor}>
      {children}
      <div className="toasts" aria-live="polite">
        {itens.map((n) => (
          <div key={n.id} className={`toast toast--${n.tipo}`} role={n.tipo === 'erro' ? 'alert' : 'status'}>
            <Icone nome={n.tipo === 'erro' ? 'alerta' : n.tipo === 'info' ? 'info' : 'check'} />
            <span>{n.mensagem}</span>
            <button type="button" className="toast__fechar" onClick={() => fechar(n.id)} aria-label="Fechar aviso">
              <Icone nome="x" />
            </button>
          </div>
        ))}
      </div>
    </NotificacoesContext.Provider>
  )
}
