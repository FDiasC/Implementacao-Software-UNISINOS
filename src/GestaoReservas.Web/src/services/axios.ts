import axios from 'axios'

const TOKEN_KEY = 'gestao-reservas:token'

export interface Credenciais {
  email: string
  senha: string
}

interface RespostaToken {
  token: string
}

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5001/api',
  headers: { 'Content-Type': 'application/json' },
})

// Transmite o token JWT para a API em todas as requisições.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Token inválido ou expirado: descarta o token salvo.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      localStorage.removeItem(TOKEN_KEY)
    }
    return Promise.reject(error)
  },
)

// Solicita o token JWT à API e o guarda para as próximas requisições.
export async function gerarToken(credenciais: Credenciais): Promise<string> {
  const { data } = await api.post<RespostaToken>('/auth/token', credenciais)
  localStorage.setItem(TOKEN_KEY, data.token)
  return data.token
}

export function removerToken(): void {
  localStorage.removeItem(TOKEN_KEY)
}
