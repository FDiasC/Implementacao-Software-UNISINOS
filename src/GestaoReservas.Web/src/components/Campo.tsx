import { useEffect, useRef, type ReactNode } from 'react'

/** Rótulo + controle + dica opcional, no padrão visual dos formulários. */
export function Campo({
  rotulo,
  dica,
  children,
  largura = 'inteira',
}: {
  rotulo: string
  dica?: ReactNode
  children: ReactNode
  largura?: 'inteira' | 'metade'
}) {
  return (
    <label className={`campo campo--${largura}`}>
      <span className="campo__rotulo">{rotulo}</span>
      {children}
      {dica && <span className="campo__dica">{dica}</span>}
    </label>
  )
}

/** Mensagem de erro da API exibida dentro do formulário (o modal continua aberto para correção). */
export function ErroFormulario({ mensagem }: { mensagem: string | null }) {
  const ref = useRef<HTMLDivElement>(null)

  // Em formulários longos o erro fica abaixo da dobra: rola até ele.
  useEffect(() => {
    if (mensagem) ref.current?.scrollIntoView({ block: 'nearest', behavior: 'smooth' })
  }, [mensagem])

  if (!mensagem) return null
  return (
    <div ref={ref} className="alerta alerta--erro" role="alert">
      {mensagem}
    </div>
  )
}
