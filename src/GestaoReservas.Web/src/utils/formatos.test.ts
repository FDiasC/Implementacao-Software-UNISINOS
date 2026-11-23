import { describe, expect, it } from 'vitest'
import type { Reserva } from '../types'
import {
  contemTexto,
  duracaoEmDias,
  formatarData,
  formatarHora,
  nomeLocal,
  paraHoraApi,
  situacaoDaReserva,
} from './formatos'

function reserva(dados: Partial<Reserva> = {}): Reserva {
  return {
    id: 1,
    usuarioId: 1,
    localId: 1,
    recursosId: [],
    dataInicial: '2026-11-25',
    horaInicial: '08:00:00',
    dataFinal: '2026-11-25',
    horaFinal: '12:00:00',
    ativo: true,
    ...dados,
  }
}

describe('formatação de data e hora', () => {
  it('converte a data da API para dd/MM/yyyy', () => {
    expect(formatarData('2026-11-05')).toBe('05/11/2026')
  })

  it('exibe a hora sem os segundos', () => {
    expect(formatarHora('08:30:00')).toBe('08:30')
  })

  it('completa os segundos da hora do formulário para a API', () => {
    expect(paraHoraApi('08:30')).toBe('08:30:00')
    expect(paraHoraApi('08:30:00')).toBe('08:30:00')
  })

  it('monta o nome do local com prédio e sala', () => {
    expect(nomeLocal({ predio: 'Bloco A', sala: '101' })).toBe('Bloco A · Sala 101')
  })
})

describe('duracaoEmDias (mesma regra do ReservaHandler)', () => {
  it('conta um período curto no mesmo dia como 1 dia', () => {
    expect(duracaoEmDias('2026-11-25', '08:00', '2026-11-25', '12:00')).toBe(1)
  })

  it('conta exatamente 24 horas como 1 dia', () => {
    expect(duracaoEmDias('2026-11-25', '08:00', '2026-11-26', '08:00')).toBe(1)
  })

  it('arredonda para cima: 1 dia e 2 horas = 2 dias', () => {
    expect(duracaoEmDias('2026-11-25', '08:00', '2026-11-26', '10:00')).toBe(2)
  })

  it('retorna null quando o término não é posterior ao início', () => {
    expect(duracaoEmDias('2026-11-25', '10:00', '2026-11-25', '10:00')).toBeNull()
    expect(duracaoEmDias('2026-11-26', '08:00', '2026-11-25', '08:00')).toBeNull()
  })

  it('retorna null enquanto o período estiver incompleto', () => {
    expect(duracaoEmDias('2026-11-25', '', '2026-11-25', '12:00')).toBeNull()
  })
})

describe('situacaoDaReserva', () => {
  const agora = new Date('2026-11-25T10:00:00')

  it('reserva inativa é cancelada, independentemente da data', () => {
    expect(situacaoDaReserva(reserva({ ativo: false }), agora)).toBe('cancelada')
  })

  it('reserva que ainda não começou é agendada', () => {
    expect(situacaoDaReserva(reserva({ dataInicial: '2026-11-26', dataFinal: '2026-11-26' }), agora)).toBe('agendada')
  })

  it('reserva que já começou e não terminou está em andamento', () => {
    expect(situacaoDaReserva(reserva(), agora)).toBe('andamento')
  })

  it('reserva cujo término já passou está encerrada', () => {
    expect(situacaoDaReserva(reserva({ horaInicial: '07:00:00', horaFinal: '09:00:00' }), agora)).toBe('encerrada')
  })
})

describe('contemTexto (busca das listagens)', () => {
  it('ignora acentos e maiúsculas', () => {
    expect(contemTexto('Auditório Central', 'AUDITORIO')).toBe(true)
  })

  it('ignora espaços nas pontas da busca', () => {
    expect(contemTexto('Sala 101', '  101 ')).toBe(true)
  })

  it('retorna false quando o texto não aparece', () => {
    expect(contemTexto('Sala 101', '202')).toBe(false)
  })
})
