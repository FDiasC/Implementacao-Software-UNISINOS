import { useMemo, useState } from 'react'
import { Campo, ErroFormulario } from '../components/Campo'
import { Confirmar, Modal } from '../components/Modal'
import { AcoesLinha, AlertaErro, BarraFiltros, BotaoNovo, CabecalhoPagina, EstadoVazio, Selo, SeloAtivo } from '../components/Pagina'
import { Tabela } from '../components/Tabela'
import { useAuth } from '../contexts/auth'
import { useCarregar } from '../hooks/useCarregar'
import { useConfirmacao, useFormulario } from '../hooks/useFormulario'
import { usuariosApi } from '../services/api'
import type { Usuario } from '../types'
import { contemTexto } from '../utils/formatos'

const VAZIO = { nome: '', email: '', senha: '' }

export function Usuarios() {
  const { usuario: logado, atualizarUsuario, sair } = useAuth()
  const [mostrarInativos, setMostrarInativos] = useState(false)
  const [busca, setBusca] = useState('')
  const lista = useCarregar(() => usuariosApi.listar(!mostrarInativos), [] as Usuario[], [mostrarInativos])
  const form = useFormulario(VAZIO)
  const confirmacao = useConfirmacao<Usuario>()

  const visiveis = useMemo(
    () => lista.dados.filter((u) => contemTexto(`${u.nome} ${u.email}`, busca)),
    [lista.dados, busca],
  )

  async function salvar() {
    const { editandoId, valores } = form
    const nome = valores.nome.trim()
    const email = valores.email.trim()
    const ok = await form.enviar(async () => {
      if (editandoId) {
        // Senha em branco na edição = mantém a atual.
        const atualizado = await usuariosApi.atualizar(editandoId, { nome, email, senha: valores.senha || null })
        if (atualizado.id === logado?.id) atualizarUsuario(atualizado)
      } else {
        await usuariosApi.criar({ nome, email, senha: valores.senha })
      }
    }, editandoId ? 'Usuário atualizado.' : 'Usuário cadastrado.')
    if (ok) void lista.recarregar()
  }

  const desativandoASiMesmo = confirmacao.alvo?.id === logado?.id

  return (
    <section className="pagina">
      <CabecalhoPagina
        titulo="Usuários"
        descricao="Pessoas que podem entrar no sistema e fazer reservas."
        acao={<BotaoNovo texto="Novo usuário" onClick={() => form.abrirNovo()} />}
      />

      <BarraFiltros
        busca={busca}
        aoBuscar={setBusca}
        placeholder="Buscar por nome ou e-mail…"
        mostrarInativos={mostrarInativos}
        aoAlternarInativos={setMostrarInativos}
      />

      {lista.erro && <AlertaErro mensagem={lista.erro} aoTentarNovamente={lista.recarregar} />}

      <Tabela
        itens={visiveis}
        carregando={lista.carregando}
        colunas={[
          {
            titulo: 'Nome',
            render: (u) => (
              <span className="celula-com-selo">
                <strong>{u.nome}</strong>
                {u.id === logado?.id && <Selo tom="info">Você</Selo>}
              </span>
            ),
          },
          { titulo: 'E-mail', render: (u) => u.email },
          { titulo: 'Situação', render: (u) => <SeloAtivo ativo={u.ativo} /> },
        ]}
        acoes={(u) =>
          u.ativo && (
            <AcoesLinha
              aoEditar={() => form.abrirEdicao(u.id, { nome: u.nome, email: u.email, senha: '' })}
              aoDesativar={() => confirmacao.pedir(u)}
            />
          )
        }
        vazio={<EstadoVazio icone="usuarios" titulo="Nenhum usuário encontrado" />}
      />

      <Modal
        aberto={form.aberto}
        titulo={form.editandoId ? 'Editar usuário' : 'Novo usuário'}
        aoFechar={form.fechar}
        rodape={
          <>
            <button type="button" className="botao botao--secundario" onClick={form.fechar}>
              Cancelar
            </button>
            <button type="submit" form="form-usuario" className="botao botao--primario" disabled={form.salvando}>
              {form.salvando ? 'Salvando…' : 'Salvar'}
            </button>
          </>
        }
      >
        <form
          id="form-usuario"
          className="formulario"
          onSubmit={(e) => {
            e.preventDefault()
            void salvar()
          }}
        >
          <Campo rotulo="Nome">
            <input required maxLength={150} autoFocus value={form.valores.nome} onChange={(e) => form.alterar('nome', e.target.value)} />
          </Campo>
          <Campo rotulo="E-mail" dica="Único no sistema; usado para entrar.">
            <input type="email" required maxLength={200} value={form.valores.email} onChange={(e) => form.alterar('email', e.target.value)} />
          </Campo>
          <Campo
            rotulo={form.editandoId ? 'Nova senha' : 'Senha'}
            dica={form.editandoId ? 'Deixe em branco para manter a senha atual.' : 'Mínimo de 4 caracteres.'}
          >
            <input
              type="password"
              autoComplete="new-password"
              required={!form.editandoId}
              minLength={4}
              maxLength={100}
              value={form.valores.senha}
              onChange={(e) => form.alterar('senha', e.target.value)}
            />
          </Campo>
          <ErroFormulario mensagem={form.erro} />
        </form>
      </Modal>

      <Confirmar
        aberto={confirmacao.alvo !== null}
        titulo="Desativar usuário"
        mensagem={
          desativandoASiMesmo ? (
            <>Você está desativando a sua própria conta. Sua sessão será encerrada e não será possível entrar novamente com ela.</>
          ) : (
            <>
              Desativar <strong>{confirmacao.alvo?.nome}</strong>? A pessoa não poderá mais entrar nem fazer reservas.
            </>
          )
        }
        textoConfirmar="Desativar"
        processando={confirmacao.processando}
        aoCancelar={confirmacao.cancelar}
        aoConfirmar={async () => {
          const ok = await confirmacao.executar((u) => usuariosApi.desativar(u.id), 'Usuário desativado.')
          if (ok && desativandoASiMesmo) sair()
          else if (ok) void lista.recarregar()
        }}
      />
    </section>
  )
}
