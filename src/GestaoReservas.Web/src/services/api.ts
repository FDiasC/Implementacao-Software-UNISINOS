import axios from 'axios'
import { api, removerToken, salvarToken } from './axios'
import type {
  AtualizarReserva,
  AtualizarUsuario,
  Categoria,
  Credenciais,
  CriarReserva,
  CriarUsuario,
  Local,
  Recurso,
  Reserva,
  SalvarCategoria,
  SalvarLocal,
  SalvarRecurso,
  Token,
  Usuario,
} from '../types'

/**
 * Cliente para um cadastro padrão da API: GET (lista e por id), POST, PUT e DELETE lógico.
 * `parametroAtivos` é o nome do filtro de ativos na rota (`apenasAtivas` em categorias).
 */
function cadastro<T, TCriar, TAtualizar = TCriar>(caminho: string, parametroAtivos = 'apenasAtivos') {
  return {
    listar: async (apenasAtivos = true) =>
      (await api.get<T[]>(caminho, { params: { [parametroAtivos]: apenasAtivos } })).data,
    obter: async (id: number) => (await api.get<T>(`${caminho}/${id}`)).data,
    criar: async (dados: TCriar) => (await api.post<T>(caminho, dados)).data,
    atualizar: async (id: number, dados: TAtualizar) => (await api.put<T>(`${caminho}/${id}`, dados)).data,
    desativar: async (id: number) => {
      await api.delete(`${caminho}/${id}`)
    },
  }
}

export const usuariosApi = cadastro<Usuario, CriarUsuario, AtualizarUsuario>('/usuarios')
export const categoriasApi = cadastro<Categoria, SalvarCategoria>('/categorias', 'apenasAtivas')
export const locaisApi = cadastro<Local, SalvarLocal>('/locais')
export const recursosApi = cadastro<Recurso, SalvarRecurso>('/recursos')
export const reservasApi = cadastro<Reserva, CriarReserva, AtualizarReserva>('/reservas')

export const authApi = {
  /** Solicita o token JWT e o guarda para as próximas requisições. */
  async entrar(credenciais: Credenciais): Promise<Token> {
    const { data } = await api.post<Token>('/auth/token', credenciais)
    salvarToken(data.token)
    return data
  },
  /** Usuário dono do token salvo (valida a sessão ao abrir a aplicação). */
  async usuarioLogado(): Promise<Usuario> {
    return (await api.get<Usuario>('/auth/me')).data
  },
  sair(): void {
    removerToken()
  },
}

/** Converte o erro do axios (ProblemDetails da API) numa mensagem para exibir. */
export function mensagemDeErro(erro: unknown): string {
  if (axios.isAxiosError(erro)) {
    if (!erro.response) return 'Não foi possível conectar à API. Verifique se ela está em execução.'
    const corpo = erro.response.data as
      | { detail?: string; title?: string; errors?: Record<string, string[]> }
      | undefined
    // 400 automático do ModelState: título genérico, causa real em "errors" por campo.
    if (corpo?.errors) return Object.values(corpo.errors).flat().join(' ')
    return corpo?.detail ?? corpo?.title ?? `A API retornou o erro ${erro.response.status}.`
  }
  return 'Erro inesperado.'
}
