import { act, renderHook, screen } from '@testing-library/react'
import { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { NotificacoesProvider } from '../contexts/NotificacoesProvider'
import { useConfirmacao, useFormulario } from './useFormulario'

const VAZIO = { nome: '', tipo: 'Local' }

const comNotificacoes = ({ children }: { children: ReactNode }) => <NotificacoesProvider>{children}</NotificacoesProvider>

function erroDaApi(titulo: string) {
  const config = { headers: {} } as InternalAxiosRequestConfig
  return new AxiosError('erro', 'ERR_BAD_REQUEST', config, null, {
    status: 400,
    statusText: '',
    data: { title: titulo },
    headers: {},
    config,
  })
}

describe('useFormulario', () => {
  it('abre vazio para um novo cadastro e com os dados atuais para edição', () => {
    const { result } = renderHook(() => useFormulario(VAZIO), { wrapper: comNotificacoes })

    act(() => result.current.abrirNovo())
    expect(result.current.aberto).toBe(true)
    expect(result.current.editandoId).toBeNull()
    expect(result.current.valores).toEqual(VAZIO)

    act(() => result.current.abrirEdicao(5, { nome: 'Auditórios', tipo: 'Local' }))
    expect(result.current.editandoId).toBe(5)
    expect(result.current.valores.nome).toBe('Auditórios')
  })

  it('ao salvar com sucesso fecha o modal e mostra o aviso', async () => {
    const { result } = renderHook(() => useFormulario(VAZIO), { wrapper: comNotificacoes })
    act(() => result.current.abrirNovo())

    let ok = false
    await act(async () => {
      ok = await result.current.enviar(() => Promise.resolve(), 'Categoria cadastrada.')
    })

    expect(ok).toBe(true)
    expect(result.current.aberto).toBe(false)
    expect(screen.getByText('Categoria cadastrada.')).toBeInTheDocument()
  })

  it('quando a API recusa, mantém o modal aberto com a mensagem de erro', async () => {
    const { result } = renderHook(() => useFormulario(VAZIO), { wrapper: comNotificacoes })
    act(() => result.current.abrirNovo())

    await act(async () => {
      await result.current.enviar(() => Promise.reject(erroDaApi('Já existe uma categoria com esse nome.')), 'ok')
    })

    expect(result.current.aberto).toBe(true)
    expect(result.current.erro).toBe('Já existe uma categoria com esse nome.')
    expect(result.current.salvando).toBe(false)
  })

  it('editar um campo limpa o erro da tentativa anterior', async () => {
    const { result } = renderHook(() => useFormulario(VAZIO), { wrapper: comNotificacoes })
    act(() => result.current.abrirNovo())
    await act(async () => {
      await result.current.enviar(() => Promise.reject(erroDaApi('Nome inválido.')), 'ok')
    })

    act(() => result.current.alterar('nome', 'Laboratórios'))

    expect(result.current.erro).toBeNull()
    expect(result.current.valores.nome).toBe('Laboratórios')
  })
})

describe('useConfirmacao', () => {
  it('executa a ação sobre o item escolhido e fecha a confirmação', async () => {
    const acao = vi.fn().mockResolvedValue(undefined)
    const { result } = renderHook(() => useConfirmacao<{ id: number }>(), { wrapper: comNotificacoes })

    act(() => result.current.pedir({ id: 3 }))
    await act(async () => {
      await result.current.executar(acao, 'Local desativado.')
    })

    expect(acao).toHaveBeenCalledWith({ id: 3 })
    expect(result.current.alvo).toBeNull()
    expect(screen.getByText('Local desativado.')).toBeInTheDocument()
  })

  it('mostra o motivo quando uma regra de negócio impede a ação', async () => {
    const { result } = renderHook(() => useConfirmacao<{ id: number }>(), { wrapper: comNotificacoes })

    act(() => result.current.pedir({ id: 3 }))
    let ok = true
    await act(async () => {
      ok = await result.current.executar(
        () => Promise.reject(erroDaApi('O local não pode ser desativado enquanto houver reservas ativas.')),
        'ok',
      )
    })

    expect(ok).toBe(false)
    expect(screen.getByRole('alert')).toHaveTextContent('O local não pode ser desativado enquanto houver reservas ativas.')
  })
})
