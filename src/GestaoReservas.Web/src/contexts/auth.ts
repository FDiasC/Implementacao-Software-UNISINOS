import { createContext, useContext } from 'react'
import type { Credenciais, Usuario } from '../types'

export interface AuthContextValue {
  /** Usuário logado; nulo enquanto não houver sessão. */
  usuario: Usuario | null
  /** true enquanto valida um token salvo de uma sessão anterior. */
  verificando: boolean
  entrar: (credenciais: Credenciais) => Promise<void>
  sair: () => void
  /** Atualiza os dados exibidos do usuário logado (ex.: após editar o próprio cadastro). */
  atualizarUsuario: (usuario: Usuario) => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth deve ser usado dentro de <AuthProvider>.')
  return ctx
}
