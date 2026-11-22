import { useEffect, useState, type ReactNode } from 'react'
import { useAuth } from '../contexts/auth'
import { linkPara, type Rota } from '../hooks/useRota'
import { Icone, type NomeIcone } from './Icone'
import { BotaoTema } from './Tema'

const MENU: { rota: Rota; rotulo: string; icone: NomeIcone }[] = [
  { rota: 'inicio', rotulo: 'Início', icone: 'inicio' },
  { rota: 'reservas', rotulo: 'Reservas', icone: 'calendario' },
  { rota: 'locais', rotulo: 'Locais', icone: 'predio' },
  { rota: 'recursos', rotulo: 'Recursos', icone: 'caixa' },
  { rota: 'categorias', rotulo: 'Categorias', icone: 'etiqueta' },
  { rota: 'usuarios', rotulo: 'Usuários', icone: 'usuarios' },
]

const iniciais = (nome: string) =>
  nome
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]!.toUpperCase())
    .join('')

/** Estrutura das telas autenticadas: menu lateral (gaveta no celular) + conteúdo. */
export function Layout({ rota, children }: { rota: Rota; children: ReactNode }) {
  const { usuario, sair } = useAuth()
  const [menuAberto, setMenuAberto] = useState(false)

  // Fecha a gaveta ao navegar.
  useEffect(() => setMenuAberto(false), [rota])

  return (
    <div className={`app${menuAberto ? ' app--menu-aberto' : ''}`}>
      <header className="barra-topo">
        <button
          type="button"
          className="botao botao--icone botao--fantasma"
          onClick={() => setMenuAberto(true)}
          aria-label="Abrir menu"
        >
          <Icone nome="menu" />
        </button>
        <span className="marca marca--compacta">
          <span className="marca__logo">
            <Icone nome="calendario" />
          </span>
          Gestão de Reservas
        </span>
        <BotaoTema />
      </header>

      <div className="app__fundo" onClick={() => setMenuAberto(false)} aria-hidden="true" />

      <aside className="lateral">
        <div className="lateral__topo">
          <span className="marca">
            <span className="marca__logo">
              <Icone nome="calendario" />
            </span>
            <span>
              Gestão de Reservas
              <small>Locais e recursos</small>
            </span>
          </span>
        </div>

        <nav className="lateral__nav" aria-label="Menu principal">
          {MENU.map((item) => (
            <a
              key={item.rota}
              href={linkPara(item.rota)}
              className={`nav-item${rota === item.rota ? ' nav-item--ativo' : ''}`}
              aria-current={rota === item.rota ? 'page' : undefined}
            >
              <Icone nome={item.icone} />
              {item.rotulo}
            </a>
          ))}
        </nav>

        <div className="lateral__rodape">
          {usuario && (
            <div className="perfil">
              <span className="avatar" aria-hidden="true">
                {iniciais(usuario.nome)}
              </span>
              <span className="perfil__texto">
                <strong>{usuario.nome}</strong>
                <small>{usuario.email}</small>
              </span>
            </div>
          )}
          <div className="lateral__botoes">
            <BotaoTema />
            <button type="button" className="botao botao--fantasma botao--pequeno" onClick={sair}>
              <Icone nome="sair" />
              Sair
            </button>
          </div>
        </div>
      </aside>

      <main className="conteudo">{children}</main>
    </div>
  )
}
