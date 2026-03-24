<template>
  <div class="fixed bottom-8 left-1/2 -translate-x-1/2 z-50 flex flex-col gap-3 pointer-events-none items-center">
    <TransitionGroup name="toast-list">
      <ToastNotification
        v-for="toast in toastStore.toasts"
        :key="toast.id"
        :message="toast.message"
        :duration="toast.duration"
        :type="toast.type"
      />
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import { useToastStore } from '@/stores/toast' // Ellenőrizd, hogy a @/ jó-e nálad!
import ToastNotification from './ToastNotification.vue'

const toastStore = useToastStore()
</script>

<style scoped>
/* Lista animációk az értesítések felbukkanásához és eltűnéséhez */
.toast-list-enter-active,
.toast-list-leave-active {
  transition: all 0.4s ease;
}

/* Alulról (Y tengelyen) csúszik be */
.toast-list-enter-from {
  opacity: 0;
  transform: translateY(30px);
}

/* Kicsinyedve és lefelé visszacsúszva tűnik el */
.toast-list-leave-to {
  opacity: 0;
  transform: translateY(30px) scale(0.9);
}
</style>
