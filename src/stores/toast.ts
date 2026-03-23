import { defineStore } from 'pinia'
import { ref } from 'vue'

// 1. Definiáljuk a lehetséges típusokat (Siker, Hiba, Figyelmeztetés)
export type ToastType = 'success' | 'error' | 'warning';

export interface ToastMessage {
  id: number;
  message: string;
  duration: number;
  type: ToastType; // 2. Hozzáadjuk a típust az interfészhez
}

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<ToastMessage[]>([])
  let nextId: number = 0

  // 3. Frissítjük az addToast függvényt:
  // Most már elfogadja a típust harmadik paraméterként,
  // és alapértelmezetten 'success' (zöld).
  const addToast = (
    message: string,
    duration: number = 3000,
    type: ToastType = 'success'
  ): void => {
    const id = nextId++

    // 4. Hozzáadjuk a típust is az objektumhoz
    toasts.value.push({ id, message, duration, type })

    setTimeout(() => {
      removeToast(id)
    }, duration)
  }

  const removeToast = (id: number): void => {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }

  return { toasts, addToast, removeToast }
})
