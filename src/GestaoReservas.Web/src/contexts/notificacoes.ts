import { createContext, useContext } from 'react'

export type TipoNotificacao = 'sucesso' | 'erro' | 'info'

export interface NotificacoesContextValue {
  notificar: (mensagem: string, tipo?: TipoNotificacao) => void
}

export const NotificacoesContext = createContext<NotificacoesContextValue | null>(null)

export function useNotificacoes(): NotificacoesContextValue {
  const ctx = useContext(NotificacoesContext)
  if (!ctx) throw new Error('useNotificacoes deve ser usado dentro de <NotificacoesProvider>.')
  return ctx
}
