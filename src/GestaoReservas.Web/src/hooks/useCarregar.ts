import { useCallback, useEffect, useRef, useState } from 'react'
import { mensagemDeErro } from '../services/api'

/**
 * Executa `buscar` ao montar e sempre que `dependencias` mudarem; expõe dados, estado de carga,
 * erro e `recarregar` (para depois de criar/editar/desativar).
 */
export function useCarregar<T>(buscar: () => Promise<T>, inicial: T, dependencias: unknown[] = []) {
  const [dados, setDados] = useState<T>(inicial)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const buscarRef = useRef(buscar)
  buscarRef.current = buscar

  const recarregar = useCallback(async () => {
    setCarregando(true)
    try {
      setDados(await buscarRef.current())
      setErro(null)
    } catch (e) {
      setErro(mensagemDeErro(e))
    } finally {
      setCarregando(false)
    }
  }, [])

  useEffect(() => {
    void recarregar()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [recarregar, ...dependencias])

  return { dados, carregando, erro, recarregar }
}
