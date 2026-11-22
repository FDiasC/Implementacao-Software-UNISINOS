import { useEffect, useState } from 'react'

export const ROTAS = ['inicio', 'reservas', 'locais', 'recursos', 'categorias', 'usuarios'] as const
export type Rota = (typeof ROTAS)[number]

function lerRota(): Rota {
  const atual = window.location.hash.replace(/^#\/?/, '')
  return (ROTAS as readonly string[]).includes(atual) ? (atual as Rota) : 'inicio'
}

/**
 * Navegação simples por hash (#/reservas, #/locais...). Evita uma dependência de roteador
 * para seis telas e mantém o botão "voltar" do navegador funcionando.
 */
export function useRota(): Rota {
  const [rota, setRota] = useState<Rota>(lerRota)

  useEffect(() => {
    const aoMudar = () => setRota(lerRota())
    window.addEventListener('hashchange', aoMudar)
    return () => window.removeEventListener('hashchange', aoMudar)
  }, [])

  return rota
}

export const linkPara = (rota: Rota) => `#/${rota}`
