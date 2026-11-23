import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthContext, type AuthContextValue } from '../contexts/auth'
import { Login } from './Login'

const { criarUsuario } = vi.hoisted(() => ({ criarUsuario: vi.fn() }))

vi.mock('../services/api', async (original) => ({
  ...(await original<typeof import('../services/api')>()),
  usuariosApi: { criar: criarUsuario },
}))

function renderizar(entrar: AuthContextValue['entrar']) {
  const valor: AuthContextValue = {
    usuario: null,
    verificando: false,
    entrar,
    sair: vi.fn(),
    atualizarUsuario: vi.fn(),
  }
  return render(
    <AuthContext.Provider value={valor}>
      <Login />
    </AuthContext.Provider>,
  )
}

function erro401(titulo: string) {
  const config = { headers: {} } as InternalAxiosRequestConfig
  return new AxiosError('erro', 'ERR_BAD_REQUEST', config, null, {
    status: 401,
    statusText: '',
    data: { title: titulo },
    headers: {},
    config,
  })
}

describe('Login', () => {
  beforeEach(() => {
    criarUsuario.mockReset()
  })

  it('entra com o e-mail e a senha digitados', async () => {
    const entrar = vi.fn().mockResolvedValue(undefined)
    renderizar(entrar)

    await userEvent.type(screen.getByLabelText('E-mail'), 'ana@teste.com')
    await userEvent.type(screen.getByLabelText('Senha'), '1234')
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(entrar).toHaveBeenCalledWith({ email: 'ana@teste.com', senha: '1234' })
    expect(criarUsuario).not.toHaveBeenCalled()
  })

  it('mostra a mensagem da API quando as credenciais são inválidas', async () => {
    renderizar(vi.fn().mockRejectedValue(erro401('E-mail ou senha inválidos.')))

    await userEvent.type(screen.getByLabelText('E-mail'), 'ana@teste.com')
    await userEvent.type(screen.getByLabelText('Senha'), 'errada')
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('E-mail ou senha inválidos.')
    expect(screen.getByRole('button', { name: 'Entrar' })).toBeEnabled()
  })

  it('na aba "Criar conta" cadastra o usuário e já entra', async () => {
    const entrar = vi.fn().mockResolvedValue(undefined)
    criarUsuario.mockResolvedValue({ id: 1, nome: 'Ana Teste', email: 'ana@teste.com', ativo: true })
    renderizar(entrar)

    await userEvent.click(screen.getByRole('tab', { name: 'Criar conta' }))
    await userEvent.type(screen.getByLabelText('Nome'), 'Ana Teste')
    await userEvent.type(screen.getByLabelText('E-mail'), 'ana@teste.com')
    await userEvent.type(screen.getByLabelText(/^Senha/), '1234')
    await userEvent.click(screen.getByRole('button', { name: 'Criar conta e entrar' }))

    expect(criarUsuario).toHaveBeenCalledWith({ nome: 'Ana Teste', email: 'ana@teste.com', senha: '1234' })
    expect(entrar).toHaveBeenCalledWith({ email: 'ana@teste.com', senha: '1234' })
  })
})
