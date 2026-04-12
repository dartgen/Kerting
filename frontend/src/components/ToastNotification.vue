<template>
  <div
    class="relative flex items-center gap-4 px-6 pt-4 pb-7 backdrop-blur-md w-max min-w-[380px] rounded-2xl pointer-events-auto transition-all duration-300"
    :class="dynamicStyles.container"
  >
    <svg v-if="type === 'success'" class="w-8 h-8 shrink-0" :class="dynamicStyles.iconColor" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
    </svg>

    <svg v-else-if="type === 'error'" class="w-8 h-8 shrink-0" :class="dynamicStyles.iconColor" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
    </svg>

    <svg v-else-if="type === 'warning'" class="w-8 h-8 shrink-0" :class="dynamicStyles.iconColor" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
    </svg>

    <span class="font-medium text-lg text-earth-50">
      {{ message }}
    </span>

    <div class="absolute bottom-2 left-6 right-6">
      <div
        class="h-1.5 rounded-full animate-shrink origin-left"
        :class="dynamicStyles.progress"
        :style="{ animationDuration: `${duration}ms` }"
      ></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { ToastType } from '@/stores/toast';

const props = defineProps<{
  message: string;
  duration: number;
  type: ToastType;
}>()

// A toast tipusa alapjan egységesen adjuk vissza a kontener/icon/progress osztalyokat.
const dynamicStyles = computed(() => {
  const styles = {
    success: {
      container: 'bg-earth-900/90 border-2 border-green-500/50 shadow-[0_8px_30px_rgba(34,197,94,0.15)]',
      iconColor: 'text-green-400',
      progress: 'bg-green-500/80',
    },
    error: {
      container: 'bg-gradient-to-br from-earth-900/95 to-red-950/60 border-2 border-red-500/50 shadow-[0_8px_30px_rgba(239,68,68,0.2)]',
      iconColor: 'text-red-400',
      progress: 'bg-red-500/80',
    },
    warning: {
      container: 'bg-gradient-to-br from-earth-900/95 to-amber-950/60 border-2 border-amber-500/50 shadow-[0_8px_30px_rgba(245,158,11,0.2)]',
      iconColor: 'text-amber-400',
      progress: 'bg-amber-400/80',
    },
  };

  return styles[props.type] || styles.success;
});
</script>

<style scoped>
@keyframes shrink {
  from { transform: scaleX(1); }
  to { transform: scaleX(0); }
}

.animate-shrink {
  animation: shrink linear forwards;
}
</style>
