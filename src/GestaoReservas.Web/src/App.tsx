import { Layout } from './components/Layout'
import { useAuth } from './contexts/auth'
import { useRota, type Rota } from './hooks/useRota'
import { Categorias } from './pages/Categorias'
import { Inicio } from './pages/Inicio'
import { Locais } from './pages/Locais'
import { Login } from './pages/Login'
import { Recursos } from './pages/Recursos'
import { Reservas } from './pages/Reservas'
import { Usuarios } from './pages/Usuarios'

const PAGINAS: Record<Rota, () => React.JSX.Element> = {
  inicio: Inicio,
  reservas: Reservas,
  locais: Locais,
  recursos: Recursos,
  categorias: Categorias,
  usuarios: Usuarios,
}

function App() {
  const { usuario, verificando } = useAuth()
  const rota = useRota()

  if (verificando) {
    return (
      <div className="tela-cheia">
        <span className="spinner" aria-hidden="true" />
        <span className="sr-only">Verificando sessão…</span>
      </div>
    )
  }

  if (!usuario) return <Login />

  const Pagina = PAGINAS[rota]
  return (
    <Layout rota={rota}>
      {/* key: troca de tela remonta a página e recarrega os dados */}
      <Pagina key={rota} />
    </Layout>
  )
}

export default App
