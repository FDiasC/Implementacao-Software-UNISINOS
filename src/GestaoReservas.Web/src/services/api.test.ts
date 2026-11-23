import { AxiosError, type AxiosAdapter, type InternalAxiosRequestConfig } from 'axios'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { authApi, categoriasApi, locaisApi, mensagemDeErro } from './api'
import { api, obterToken } from './axios'

function erroHttp(status: number, data: unknown) {
  const config = { headers: {} } as InternalAxiosRequestConfig
  return new AxiosError('erro', 'ERR_BAD_REQUEST', config, null, {
    status,
    statusText: '',
    data,
    headers: {},
    config,
  })
}

describe('mensagemDeErro', () => {
  it('usa o título do ProblemDetails devolvido pela API', () => {
    const erro = erroHttp(409, { title: "O local '101' já está reservado no período informado." })
    expect(mensagemDeErro(erro)).toBe("O local '101' já está reservado no período informado.")
  })

  it('junta as mensagens de validação por campo (400 automático)', () => {
    const erro = erroHttp(400, {
      title: 'One or more validation errors occurred.',
      errors: { Nome: ['O campo Nome é obrigatório.'], Email: ['O campo Email não é um e-mail válido.'] },
    })
    expect(mensagemDeErro(erro)).toBe('O campo Nome é obrigatório. O campo Email não é um e-mail válido.')
  })

  it('informa o código HTTP quando não há corpo', () => {
    expect(mensagemDeErro(erroHttp(500, undefined))).toBe('A API retornou o erro 500.')
  })

  it('avisa quando a API está fora do ar (sem resposta)', () => {
    const semResposta = new AxiosError('Network Error', 'ERR_NETWORK')
    expect(mensagemDeErro(semResposta)).toMatch(/Não foi possível conectar à API/)
  })

  it('trata erros que não vieram do axios', () => {
    expect(mensagemDeErro(new Error('qualquer'))).toBe('Erro inesperado.')
  })
})

describe('chamadas dos cadastros', () => {
  const adapterOriginal = api.defaults.adapter
  let requisicoes: InternalAxiosRequestConfig[]

  beforeEach(() => {
    requisicoes = []
    const adapter: AxiosAdapter = async (config) => {
      requisicoes.push(config)
      return { data: [], status: 200, statusText: 'OK', headers: {}, config }
    }
    api.defaults.adapter = adapter
  })

  afterEach(() => {
    api.defaults.adapter = adapterOriginal
  })

  it('envia o filtro de ativos com o nome esperado por cada rota', async () => {
    await locaisApi.listar(false)
    await categoriasApi.listar(true)

    expect(requisicoes[0].url).toBe('/locais')
    expect(requisicoes[0].params).toEqual({ apenasAtivos: false })
    expect(requisicoes[1].url).toBe('/categorias')
    expect(requisicoes[1].params).toEqual({ apenasAtivas: true })
  })

  it('usa os verbos e rotas REST para criar, atualizar e desativar', async () => {
    const dados = { sala: '101', predio: 'A', capacidade: 10, andar: 1, permiteRecursos: true, categoriaId: 1 }
    await locaisApi.criar(dados)
    await locaisApi.atualizar(7, dados)
    await locaisApi.desativar(7)

    expect(requisicoes.map((r) => [r.method, r.url])).toEqual([
      ['post', '/locais'],
      ['put', '/locais/7'],
      ['delete', '/locais/7'],
    ])
    expect(JSON.parse(requisicoes[0].data as string)).toEqual(dados)
  })

  it('guarda o token recebido no login', async () => {
    api.defaults.adapter = async (config) => ({
      data: { token: 'abc.def.ghi', expiraEm: '2026-11-24T00:00:00Z', usuario: { id: 1, nome: 'Ana', email: 'ana@x.com', ativo: true } },
      status: 200,
      statusText: 'OK',
      headers: {},
      config,
    })

    const resposta = await authApi.entrar({ email: 'ana@x.com', senha: '1234' })

    expect(resposta.usuario.nome).toBe('Ana')
    expect(obterToken()).toBe('abc.def.ghi')
  })
})
