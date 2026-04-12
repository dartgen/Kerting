import { defineStore } from 'pinia'
import { ref } from 'vue'

// Megjeleníthető toast típusok.
export type ToastType = 'success' | 'error' | 'warning';

export interface ToastMessage {
  id: number;
  message: string;
  duration: number;
  type: ToastType;
}

// Toast store:
// rövid életidejű UI üzenetek (siker, hiba, figyelmeztetés) kezelésére.
export const useToastStore = defineStore('toast', () => {
  const toasts = ref<ToastMessage[]>([])

  // Egyszerű, növekvő azonosító a listakezeléshez.
  let nextId: number = 0

  // Új toast felvétele automatikus időzített eltávolítással.
  const addToast = (
    message: string,
    duration: number = 3000,
    type: ToastType = 'success'
  ): void => {
    const id = nextId++

    // A teljes toast objektumot felvesszük a megjelenítési sorba.
    toasts.value.push({ id, message, duration, type })

    // Lejárat után automatikus törlés, hogy ne maradjon elavult üzenet a felületen.
    setTimeout(() => {
      removeToast(id)
    }, duration)
  }

  // Egy konkrét toast eltávolítása ID alapján.
  const removeToast = (id: number): void => {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }

  return { toasts, addToast, removeToast }
})
