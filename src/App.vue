<script setup lang="ts">
import HeaderComponent from './components/headerComponent.vue'
import bgImage from './assets/background.jpg'
</script>

<template>
  <div
    class="min-h-screen text-earth-100 flex flex-col relative overflow-hidden bg-cover bg-center bg-no-repeat"
    :style="{ backgroundImage: `url(${bgImage})` }"
  >
    <div class="absolute top-0 left-0 w-full h-full bg-black/40 backdrop-blur-[2px]"></div>
    <div class="relative z-10 flex flex-col min-h-screen">
      <HeaderComponent class="sticky top-0 z-50 transition-transform duration-300" />
      <main class="grow container mx-auto px-4 py-6 sm:py-8 flex flex-col">
        <RouterView v-slot="{ Component }">
          <transition name="fade" mode="out-in">
             <component :is="Component" />
          </transition>
        </RouterView>
      </main>
      <footer class="py-6 text-center text-earth-200/60 text-sm">
        &copy; {{ new Date().getFullYear() }} Kerting. Minden jog fenntartva.
      </footer>
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
