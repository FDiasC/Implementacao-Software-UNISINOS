// Espelho das DTOs da API (GestaoReservas.Domain/Dtos). Datas em yyyy-MM-dd e horas em HH:mm:ss.

export type TipoCategoria = 'Local' | 'Recurso'

export interface Usuario {
  id: number
  nome: string
  email: string
  ativo: boolean
}

export interface CriarUsuario {
  nome: string
  email: string
  senha: string
}

export interface AtualizarUsuario {
  nome: string
  email: string
  /** Omitida ou nula: a API mantém a senha atual. */
  senha?: string | null
}

export interface Categoria {
  id: number
  nome: string
  tipo: TipoCategoria
  ativo: boolean
}

export interface SalvarCategoria {
  nome: string
  tipo: TipoCategoria
}

export interface Local {
  id: number
  sala: string
  predio: string
  capacidade: number
  andar: number
  permiteRecursos: boolean
  categoriaId: number
  categoriaNome: string
  ativo: boolean
}

export interface SalvarLocal {
  sala: string
  predio: string
  capacidade: number
  andar: number
  permiteRecursos: boolean
  categoriaId: number
}

export interface Recurso {
  id: number
  descricao: string
  numeroPatrimonio: string
  diasMinimosReserva: number
  diasMaximosReserva: number
  categoriaId: number
  categoriaNome: string
  ativo: boolean
}

export interface SalvarRecurso {
  numeroPatrimonio: string
  descricao: string
  diasMinimosReserva: number
  diasMaximosReserva: number
  categoriaId: number
}

export interface Reserva {
  id: number
  usuarioId: number
  localId: number | null
  recursosId: number[]
  dataInicial: string
  horaInicial: string
  dataFinal: string
  horaFinal: string
  ativo: boolean
}

export interface AtualizarReserva {
  localId: number | null
  dataInicial: string
  horaInicial: string
  dataFinal: string
  horaFinal: string
  recursosId: number[]
}

export interface CriarReserva extends AtualizarReserva {
  usuarioId: number
}

export interface Credenciais {
  email: string
  senha: string
}

export interface Token {
  token: string
  expiraEm: string
  usuario: Usuario
}
