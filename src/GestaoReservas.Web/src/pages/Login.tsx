import { useState, type FormEvent } from 'react'
import { ErroFormulario } from '../components/Campo'
import { Icone } from '../components/Icone'
import { useAuth } from '../contexts/auth'
import { mensagemDeErro, usuariosApi } from '../services/api'

type Modo = 'entrar' | 'cadastrar'

/** Tela de acesso: login (POST /auth/token) ou criação de conta (POST /usuarios é público). */
export function Login() {
  const { entrar } = useAuth()
  const [modo, setModo] = useState<Modo>('entrar')
  const [nome, setNome] = useState('')
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  function trocarModo(novo: Modo) {
    setModo(novo)
    setErro(null)
  }

  async function enviar(e: FormEvent) {
    e.preventDefault()
    setErro(null)
    setEnviando(true)
    try {
      if (modo === 'cadastrar') {
        await usuariosApi.criar({ nome, email, senha })
      }
      await entrar({ email, senha })
    } catch (err) {
      setErro(mensagemDeErro(err))
      setEnviando(false)
    }
  }

  return (
    <div className="acesso">
      <section className="acesso__apresentacao" aria-hidden="true">
        <span className="marca marca--grande">
          <span className="marca__logo">
            <Icone nome="calendario" tamanho={22} />
          </span>
          Gestão de Reservas
        </span>
        <p>Reserve salas, auditórios e equipamentos sem conflito de horário.</p>
        <ul>
          <li>
            <Icone nome="check" /> Locais e recursos em um só lugar
          </li>
          <li>
            <Icone nome="check" /> Verificação automática de choque de horário
          </li>
          <li>
            <Icone nome="check" /> Histórico de reservas canceladas
          </li>
        </ul>
      </section>

      <section className="acesso__painel">
        <div className="cartao acesso__cartao">
          <h1>{modo === 'entrar' ? 'Entrar' : 'Criar conta'}</h1>
          <p className="texto-suave">
            {modo === 'entrar' ? 'Use o e-mail e a senha cadastrados.' : 'Preencha seus dados para começar a reservar.'}
          </p>

          <div className="abas" role="tablist">
            <button
              type="button"
              role="tab"
              aria-selected={modo === 'entrar'}
              className={`aba${modo === 'entrar' ? ' aba--ativa' : ''}`}
              onClick={() => trocarModo('entrar')}
            >
              Entrar
            </button>
            <button
              type="button"
              role="tab"
              aria-selected={modo === 'cadastrar'}
              className={`aba${modo === 'cadastrar' ? ' aba--ativa' : ''}`}
              onClick={() => trocarModo('cadastrar')}
            >
              Criar conta
            </button>
          </div>

          <form className="formulario" onSubmit={enviar}>
            {modo === 'cadastrar' && (
              <label className="campo">
                <span className="campo__rotulo">Nome</span>
                <input required maxLength={150} autoComplete="name" value={nome} onChange={(e) => setNome(e.target.value)} />
              </label>
            )}
            <label className="campo">
              <span className="campo__rotulo">E-mail</span>
              <input
                type="email"
                required
                maxLength={200}
                autoComplete="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </label>
            <label className="campo">
              <span className="campo__rotulo">Senha</span>
              <input
                type="password"
                required
                minLength={modo === 'cadastrar' ? 4 : undefined}
                maxLength={100}
                autoComplete={modo === 'entrar' ? 'current-password' : 'new-password'}
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
              />
              {modo === 'cadastrar' && <span className="campo__dica">Mínimo de 4 caracteres.</span>}
            </label>

            <ErroFormulario mensagem={erro} />

            <button type="submit" className="botao botao--primario botao--bloco" disabled={enviando}>
              {enviando ? 'Aguarde…' : modo === 'entrar' ? 'Entrar' : 'Criar conta e entrar'}
            </button>
          </form>
        </div>
      </section>
    </div>
  )
}
