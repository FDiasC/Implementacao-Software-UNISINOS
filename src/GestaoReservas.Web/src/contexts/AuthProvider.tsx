import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '../services/api'
import { EVENTO_SESSAO_EXPIRADA, obterToken } from '../services/axios'
import type { Credenciais, Usuario } from '../types'
import { AuthContext } from './auth'
import { useNotificacoes } from './notificacoes'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(null)
  const [verificando, setVerificando] = useState(() => obterToken() !== null)
  const { notificar } = useNotificacoes()

  // Sessão anterior: confirma com a API que o token ainda vale e quem é o usuário.
  useEffect(() => {
    if (!obterToken()) return
    let ativo = true
    authApi
      .usuarioLogado()
      .then((u) => ativo && setUsuario(u))
      .catch(() => ativo && authApi.sair())
      .finally(() => ativo && setVerificando(false))
    return () => {
      ativo = false
    }
  }, [])

  // Qualquer 401 durante o uso (token expirado, usuário desativado) encerra a sessão.
  useEffect(() => {
    function aoExpirar() {
      setUsuario((atual) => {
        if (atual) notificar('Sua sessão expirou. Entre novamente.', 'erro')
        return null
      })
    }
    window.addEventListener(EVENTO_SESSAO_EXPIRADA, aoExpirar)
    return () => window.removeEventListener(EVENTO_SESSAO_EXPIRADA, aoExpirar)
  }, [notificar])

  const entrar = useCallback(async (credenciais: Credenciais) => {
    const { usuario } = await authApi.entrar(credenciais)
    setUsuario(usuario)
  }, [])

  const sair = useCallback(() => {
    authApi.sair()
    setUsuario(null)
  }, [])

  const valor = useMemo(
    () => ({ usuario, verificando, entrar, sair, atualizarUsuario: setUsuario }),
    [usuario, verificando, entrar, sair],
  )

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>
}
