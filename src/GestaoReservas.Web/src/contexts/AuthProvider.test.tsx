import { act, render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { EVENTO_SESSAO_EXPIRADA, salvarToken } from '../services/axios'
import { useAuth } from './auth'
import { AuthProvider } from './AuthProvider'
import { NotificacoesProvider } from './NotificacoesProvider'

const { authApi } = vi.hoisted(() => ({
  authApi: { usuarioLogado: vi.fn(), entrar: vi.fn(), sair: vi.fn() },
}))

vi.mock('../services/api', async (original) => ({
  ...(await original<typeof import('../services/api')>()),
  authApi,
}))

const ANA = { id: 1, nome: 'Ana Teste', email: 'ana@teste.com', ativo: true }

function Sessao() {
  const { usuario, verificando } = useAuth()
  if (verificando) return <p>verificando</p>
  return <p>{usuario ? `logado: ${usuario.nome}` : 'sem sessão'}</p>
}

function renderizar() {
  return render(
    <NotificacoesProvider>
      <AuthProvider>
        <Sessao />
      </AuthProvider>
    </NotificacoesProvider>,
  )
}

describe('AuthProvider', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('sem token salvo, vai direto para "sem sessão" sem chamar a API', () => {
    renderizar()
    expect(screen.getByText('sem sessão')).toBeInTheDocument()
    expect(authApi.usuarioLogado).not.toHaveBeenCalled()
  })

  it('com token salvo, confirma o usuário na API antes de liberar o sistema', async () => {
    salvarToken('token-salvo')
    authApi.usuarioLogado.mockResolvedValue(ANA)

    renderizar()

    expect(screen.getByText('verificando')).toBeInTheDocument()
    expect(await screen.findByText('logado: Ana Teste')).toBeInTheDocument()
  })

  it('descarta o token salvo quando a API não o aceita mais', async () => {
    salvarToken('token-vencido')
    authApi.usuarioLogado.mockRejectedValue(new Error('401'))

    renderizar()

    expect(await screen.findByText('sem sessão')).toBeInTheDocument()
    expect(authApi.sair).toHaveBeenCalled()
  })

  it('volta para o login e avisa quando a sessão expira durante o uso', async () => {
    salvarToken('token-salvo')
    authApi.usuarioLogado.mockResolvedValue(ANA)
    renderizar()
    await screen.findByText('logado: Ana Teste')

    act(() => {
      window.dispatchEvent(new Event(EVENTO_SESSAO_EXPIRADA))
    })

    expect(screen.getByText('sem sessão')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toHaveTextContent('Sua sessão expirou. Entre novamente.')
  })
})
