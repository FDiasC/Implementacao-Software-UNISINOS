import type { Local, Reserva } from '../types'

/** yyyy-MM-dd → dd/MM/yyyy */
export function formatarData(data: string): string {
  const [ano, mes, dia] = data.split('-')
  return `${dia}/${mes}/${ano}`
}

/** HH:mm:ss → HH:mm */
export const formatarHora = (hora: string) => hora.slice(0, 5)

/** HH:mm (input type=time) → HH:mm:ss (TimeOnly da API) */
export const paraHoraApi = (hora: string) => (hora.length === 5 ? `${hora}:00` : hora)

export const nomeLocal = (local: Pick<Local, 'predio' | 'sala'>) => `${local.predio} · Sala ${local.sala}`

export function inicioDaReserva(r: Pick<Reserva, 'dataInicial' | 'horaInicial'>): Date {
  return new Date(`${r.dataInicial}T${r.horaInicial}`)
}

export function fimDaReserva(r: Pick<Reserva, 'dataFinal' | 'horaFinal'>): Date {
  return new Date(`${r.dataFinal}T${r.horaFinal}`)
}

/**
 * Duração em dias arredondada para cima, igual ao ReservaHandler (1 dia e 2 horas = 2 dias).
 * Retorna null se o período estiver incompleto ou inválido.
 */
export function duracaoEmDias(dataInicial: string, horaInicial: string, dataFinal: string, horaFinal: string) {
  if (!dataInicial || !horaInicial || !dataFinal || !horaFinal) return null
  const inicio = new Date(`${dataInicial}T${paraHoraApi(horaInicial)}`)
  const fim = new Date(`${dataFinal}T${paraHoraApi(horaFinal)}`)
  const ms = fim.getTime() - inicio.getTime()
  if (!(ms > 0)) return null
  return Math.ceil(ms / 86_400_000)
}

export type SituacaoReserva = 'cancelada' | 'encerrada' | 'andamento' | 'agendada'

export function situacaoDaReserva(r: Reserva, agora = new Date()): SituacaoReserva {
  if (!r.ativo) return 'cancelada'
  if (fimDaReserva(r) <= agora) return 'encerrada'
  if (inicioDaReserva(r) <= agora) return 'andamento'
  return 'agendada'
}

export const ROTULO_SITUACAO: Record<SituacaoReserva, string> = {
  agendada: 'Agendada',
  andamento: 'Em andamento',
  encerrada: 'Encerrada',
  cancelada: 'Cancelada',
}

/** Comparação sem acento/maiúsculas para a busca das listagens. */
export function contemTexto(alvo: string, busca: string): boolean {
  const normalizar = (s: string) => s.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase()
  return normalizar(alvo).includes(normalizar(busca.trim()))
}
