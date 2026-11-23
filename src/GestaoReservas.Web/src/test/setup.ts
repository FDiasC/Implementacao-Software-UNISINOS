import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'

// O jsdom (navegador simulado dos testes) não implementa algumas APIs que o app usa:
// showModal()/close() do <dialog> (componente Modal) e scrollIntoView() (ErroFormulario).
HTMLDialogElement.prototype.showModal ??= function (this: HTMLDialogElement) {
  this.open = true
}
HTMLDialogElement.prototype.close ??= function (this: HTMLDialogElement) {
  this.open = false
}
Element.prototype.scrollIntoView ??= () => {}

afterEach(() => {
  cleanup()
  localStorage.clear()
  window.location.hash = ''
})
