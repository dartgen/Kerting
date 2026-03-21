<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import HeaderComponent from './components/headerComponent.vue'
import bgImage from './assets/background.jpg'

const route = useRoute()
const hideHeader = computed(() => Boolean(route.meta.hideHeader))
const isFullPage = computed(() => Boolean(route.meta.fullPage))
</script>

<template>
  <div
    class="w-full h-[100dvh] text-earth-100 flex flex-col relative overflow-hidden bg-cover bg-center bg-no-repeat"
    :style="{ backgroundImage: `url(${bgImage})` }"
  >
    <div class="absolute top-0 left-0 w-full h-full bg-black/40 backdrop-blur-[2px]"></div>
    <div class="relative z-10 flex flex-col h-[100dvh] w-full">
      <HeaderComponent v-if="!hideHeader" class="flex-none transition-transform duration-300 h-auto" />
      <main
        :class="[
          'flex-1 w-full mx-auto flex flex-col min-h-0 overflow-hidden',
          isFullPage ? 'px-0 pb-0' : 'px-4 pb-2 sm:pb-4',
        ]"
      >
        <RouterView v-slot="{ Component }">
          <transition name="fade" mode="out-in">
             <component :is="Component" />
          </transition>
        </RouterView>
      </main>
      </div>
    </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
