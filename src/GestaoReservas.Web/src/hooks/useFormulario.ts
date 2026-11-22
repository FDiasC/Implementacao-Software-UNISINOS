import { useCallback, useState } from 'react'
import { useNotificacoes } from '../contexts/notificacoes'
import { mensagemDeErro } from '../services/api'

/**
 * Estado do modal de cadastro/edição usado por todas as telas:
 * valores do formulário, id em edição (null = novo), erro da API e envio.
 */
export function useFormulario<TForm extends object>(vazio: TForm) {
  const [aberto, setAberto] = useState(false)
  const [editandoId, setEditandoId] = useState<number | null>(null)
  const [valores, setValores] = useState<TForm>(vazio)
  const [erro, setErro] = useState<string | null>(null)
  const [salvando, setSalvando] = useState(false)
  const { notificar } = useNotificacoes()

  const abrirNovo = useCallback(
    (iniciais?: Partial<TForm>) => {
      setValores({ ...vazio, ...iniciais })
      setEditandoId(null)
      setErro(null)
      setAberto(true)
    },
    [vazio],
  )

  const abrirEdicao = useCallback((id: number, atuais: TForm) => {
    setValores(atuais)
    setEditandoId(id)
    setErro(null)
    setAberto(true)
  }, [])

  const fechar = useCallback(() => setAberto(false), [])

  // Qualquer edição tira da tela o erro da tentativa anterior.
  const atualizarValores = useCallback((novo: TForm | ((atual: TForm) => TForm)) => {
    setValores(novo)
    setErro(null)
  }, [])

  const alterar = useCallback(<K extends keyof TForm>(campo: K, valor: TForm[K]) => {
    atualizarValores((atual) => ({ ...atual, [campo]: valor }))
  }, [atualizarValores])

  /** Envia; em caso de sucesso fecha o modal e notifica. Erros da API ficam visíveis no próprio modal. */
  const enviar = useCallback(
    async (acao: () => Promise<unknown>, mensagemSucesso: string) => {
      setSalvando(true)
      setErro(null)
      try {
        await acao()
        setAberto(false)
        notificar(mensagemSucesso)
        return true
      } catch (e) {
        setErro(mensagemDeErro(e))
        return false
      } finally {
        setSalvando(false)
      }
    },
    [notificar],
  )

  return { aberto, editandoId, valores, setValores: atualizarValores, alterar, erro, salvando, abrirNovo, abrirEdicao, fechar, enviar }
}

/** Estado da confirmação de desativação/cancelamento de um item. */
export function useConfirmacao<T>() {
  const [alvo, setAlvo] = useState<T | null>(null)
  const [processando, setProcessando] = useState(false)
  const { notificar } = useNotificacoes()

  const executar = useCallback(
    async (acao: (item: T) => Promise<unknown>, mensagemSucesso: string) => {
      if (alvo === null) return false
      setProcessando(true)
      try {
        await acao(alvo)
        notificar(mensagemSucesso)
        setAlvo(null)
        return true
      } catch (e) {
        // Regra de negócio (ex.: local com reservas ativas): mostra o motivo e mantém a lista.
        notificar(mensagemDeErro(e), 'erro')
        setAlvo(null)
        return false
      } finally {
        setProcessando(false)
      }
    },
    [alvo, notificar],
  )

  return { alvo, pedir: setAlvo, cancelar: () => setAlvo(null), processando, executar }
}
