import { act, renderHook } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { linkPara, useRota } from './useRota'

function navegarPara(hash: string) {
  act(() => {
    window.location.hash = hash
    window.dispatchEvent(new HashChangeEvent('hashchange'))
  })
}

describe('useRota', () => {
  it('lê a tela a partir do hash da URL', () => {
    window.location.hash = '#/locais'
    const { result } = renderHook(() => useRota())
    expect(result.current).toBe('locais')
  })

  it('usa "inicio" quando o hash está vazio ou não é uma tela conhecida', () => {
    window.location.hash = '#/nao-existe'
    const { result } = renderHook(() => useRota())
    expect(result.current).toBe('inicio')
  })

  it('acompanha a navegação (inclusive o botão voltar do navegador)', () => {
    const { result } = renderHook(() => useRota())

    navegarPara('#/reservas')
    expect(result.current).toBe('reservas')

    navegarPara('#/usuarios')
    expect(result.current).toBe('usuarios')
  })

  it('gera o link de cada tela', () => {
    expect(linkPara('recursos')).toBe('#/recursos')
  })
})
