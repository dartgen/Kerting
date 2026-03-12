<script setup lang="ts">
import HeaderComponent from './components/headerComponent.vue'
import bgImage from './assets/background.jpg'
</script>

<template>
  <div
    class="min-h-screen text-earth-100 flex flex-col relative overflow-hidden bg-cover bg-center bg-no-repeat"
    :style="{ backgroundImage: `url(${bgImage})` }"
  >
    <div class="absolute top-0 left-0 w-full h-full bg-black opacity-50"></div>
    <div class="relative z-10 flex flex-col grow">
      <HeaderComponent />
      <main class="grow">
        <RouterView />
      </main>
      <footer></footer>
    </div>
  </div>
</template>

<style scoped></style>
