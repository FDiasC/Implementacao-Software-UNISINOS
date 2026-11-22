import axios from 'axios'

const TOKEN_KEY = 'gestao-reservas:token'

/** Disparado em `window` quando a API recusa o token (401) — o AuthProvider volta para o login. */
export const EVENTO_SESSAO_EXPIRADA = 'gestao-reservas:sessao-expirada'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5001/api',
  headers: { 'Content-Type': 'application/json' },
})

export function obterToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function salvarToken(token: string): void {
  try {
    localStorage.setItem(TOKEN_KEY, token)
  } catch {
    // Armazenamento indisponível (ex.: navegação privada bloqueada): a sessão dura só até recarregar.
  }
}

export function removerToken(): void {
  try {
    localStorage.removeItem(TOKEN_KEY)
  } catch {
    // Sem armazenamento disponível: não há token salvo para remover.
  }
}

// Transmite o token JWT para a API em todas as requisições.
api.interceptors.request.use((config) => {
  const token = obterToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Token inválido ou expirado: descarta o token e avisa a aplicação.
// O próprio login devolve 401 para senha errada; esse caso é tratado na tela, não aqui.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401 &&
      !error.config?.url?.includes('/auth/token')
    ) {
      removerToken()
      window.dispatchEvent(new Event(EVENTO_SESSAO_EXPIRADA))
    }
    return Promise.reject(error)
  },
)
