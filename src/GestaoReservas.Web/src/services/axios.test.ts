import { AxiosError, type AxiosAdapter, type InternalAxiosRequestConfig } from 'axios'
import { afterEach, beforeEach, describe, expect, it, vi, type Mock } from 'vitest'
import { EVENTO_SESSAO_EXPIRADA, api, obterToken, removerToken, salvarToken } from './axios'

const adapterOriginal = api.defaults.adapter

function responderCom(status: number): { adapter: AxiosAdapter; requisicoes: InternalAxiosRequestConfig[] } {
  const requisicoes: InternalAxiosRequestConfig[] = []
  const adapter: AxiosAdapter = async (config) => {
    requisicoes.push(config)
    const resposta = { data: {}, status, statusText: '', headers: {}, config }
    if (status >= 400) throw new AxiosError('erro', 'ERR_BAD_REQUEST', config, null, resposta)
    return resposta
  }
  return { adapter, requisicoes }
}

describe('armazenamento do token', () => {
  it('salva, lê e remove o token', () => {
    salvarToken('meu-token')
    expect(obterToken()).toBe('meu-token')

    removerToken()
    expect(obterToken()).toBeNull()
  })
})

describe('interceptadores do axios', () => {
  let aoExpirar: Mock<(evento: Event) => void>

  beforeEach(() => {
    aoExpirar = vi.fn<(evento: Event) => void>()
    window.addEventListener(EVENTO_SESSAO_EXPIRADA, aoExpirar)
  })

  afterEach(() => {
    window.removeEventListener(EVENTO_SESSAO_EXPIRADA, aoExpirar)
    api.defaults.adapter = adapterOriginal
  })

  it('envia o token salvo no cabeçalho Authorization', async () => {
    const { adapter, requisicoes } = responderCom(200)
    api.defaults.adapter = adapter
    salvarToken('abc')

    await api.get('/reservas')

    expect(requisicoes[0].headers.Authorization).toBe('Bearer abc')
  })

  it('não envia Authorization quando não há sessão', async () => {
    const { adapter, requisicoes } = responderCom(200)
    api.defaults.adapter = adapter

    await api.get('/reservas')

    expect(requisicoes[0].headers.Authorization).toBeUndefined()
  })

  it('em 401 descarta o token e avisa que a sessão expirou', async () => {
    api.defaults.adapter = responderCom(401).adapter
    salvarToken('expirado')

    await expect(api.get('/reservas')).rejects.toBeInstanceOf(AxiosError)

    expect(obterToken()).toBeNull()
    expect(aoExpirar).toHaveBeenCalledOnce()
  })

  it('senha errada no login (401) não é tratada como sessão expirada', async () => {
    api.defaults.adapter = responderCom(401).adapter

    await expect(api.post('/auth/token', { email: 'a@b.com', senha: 'x' })).rejects.toBeInstanceOf(AxiosError)

    expect(aoExpirar).not.toHaveBeenCalled()
  })

  it('outros erros (ex.: 409) não encerram a sessão', async () => {
    api.defaults.adapter = responderCom(409).adapter
    salvarToken('valido')

    await expect(api.post('/reservas', {})).rejects.toBeInstanceOf(AxiosError)

    expect(obterToken()).toBe('valido')
    expect(aoExpirar).not.toHaveBeenCalled()
  })
})
