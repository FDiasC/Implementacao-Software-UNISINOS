import { useEffect, useState } from 'react'
import { Icone } from './Icone'

type Tema = 'claro' | 'escuro'
const CHAVE = 'gestao-reservas:tema'

function temaInicial(): Tema {
  try {
    const salvo = localStorage.getItem(CHAVE)
    if (salvo === 'claro' || salvo === 'escuro') return salvo
  } catch {
    // sem armazenamento: segue a preferência do sistema
  }
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'escuro' : 'claro'
}

/** Alterna entre tema claro e escuro (lembrado no navegador). */
export function BotaoTema() {
  const [tema, setTema] = useState<Tema>(temaInicial)

  useEffect(() => {
    document.documentElement.dataset.theme = tema === 'escuro' ? 'dark' : 'light'
    try {
      localStorage.setItem(CHAVE, tema)
    } catch {
      // ignora
    }
  }, [tema])

  // Mantém os dois botões (barra do topo e menu lateral) sincronizados.
  useEffect(() => {
    const observar = new MutationObserver(() => {
      const atual = document.documentElement.dataset.theme === 'dark' ? 'escuro' : 'claro'
      setTema((t) => (t === atual ? t : atual))
    })
    observar.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] })
    return () => observar.disconnect()
  }, [])

  const proximo = tema === 'escuro' ? 'claro' : 'escuro'
  return (
    <button
      type="button"
      className="botao botao--icone botao--fantasma"
      onClick={() => setTema(proximo)}
      title={`Usar tema ${proximo}`}
      aria-label={`Usar tema ${proximo}`}
    >
      <Icone nome={tema === 'escuro' ? 'sol' : 'lua'} />
    </button>
  )
}
