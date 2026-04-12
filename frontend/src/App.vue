<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import HeaderComponent from './components/headerComponent.vue'
import bgImage from './assets/background.jpg'

import ToastContainer from './components/ToastContainer.vue'

const route = useRoute()
// Bizonyos route-oknál (pl. login) a fejlécet elrejtjük route meta alapján.
const hideHeader = computed(() => Boolean(route.meta.hideHeader))
// Full page nézet esetén a fő tartalom nem kap alap belső margót/paddinget.
const isFullPage = computed(() => Boolean(route.meta.fullPage))
</script>

<template>
  <div
    class="w-full h-[100dvh] text-earth-100 flex flex-col relative overflow-hidden bg-cover bg-center bg-no-repeat"
    :style="{ backgroundImage: `url(${bgImage})` }"
  >
    <!-- Globális sötétítő réteg a háttérkép olvashatóságának javításához. -->
    <div class="absolute top-0 left-0 w-full h-full bg-black/40 backdrop-blur-[2px]"></div>
    <div class="relative z-10 flex flex-col h-[100dvh] w-full">
      <HeaderComponent v-if="!hideHeader" class="flex-none transition-transform duration-300 h-auto" />
      <main
        :class="[
          'flex-1 w-full mx-auto flex flex-col min-h-0 overflow-hidden',
          isFullPage ? 'px-0 pb-0' : 'px-4 pb-2 sm:pb-4',
        ]"
      >
        <RouterView />
      </main>

      <ToastContainer />

    </div>
  </div>
</template>
