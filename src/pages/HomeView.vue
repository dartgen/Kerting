<script setup lang="ts">
// No script needed for now
</script>

<template>
  <div class="container mx-auto px-4 py-8 sm:py-12 flex flex-col items-center">
    <div class="page-card w-full max-w-4xl text-center">
      <h1 class="text-4xl md:text-5xl font-serif text-earth-100 mb-6 drop-shadow-lg">
        Üdvözöljük a Kertingben!
      </h1>
      <p class="text-lg md:text-xl text-earth-200/90 mb-8 leading-relaxed max-w-2xl mx-auto">
        Fedezze fel a természet nyugalmát és a kertészkedés örömét közösségünkkel.
        Oszd meg tapasztalataidat, inspirálódj és találj új barátokra.
      </p>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-12 text-left">
        <div class="bg-earth-800/40 p-6 rounded-2xl border border-earth-100/10 hover:bg-earth-800/60 transition-colors duration-300">
          <h2 class="text-2xl font-bold text-earth-50 mb-3">🌱 Fórum</h2>
          <p class="text-earth-200/80 mb-4">Csatlakozz a beszélgetésekhez, tégy fel kérdéseket és ossz meg tippeket.</p>
          <RouterLink to="/forum" class="text-earth-400 hover:text-earth-200 font-semibold underline decoration-earth-400/50 hover:decoration-earth-200 underline-offset-4 transition-all">Irány a Fórum &rarr;</RouterLink>
        </div>

        <div class="bg-earth-800/40 p-6 rounded-2xl border border-earth-100/10 hover:bg-earth-800/60 transition-colors duration-300">
          <h2 class="text-2xl font-bold text-earth-50 mb-3">🖼 Galéria</h2>
          <p class="text-earth-200/80 mb-4">Nézd meg tagjaink legszebb kertjeit és töltsd fel saját büszkeségeidet.</p>
          <RouterLink to="/gallery" class="text-earth-400 hover:text-earth-200 font-semibold underline decoration-earth-400/50 hover:decoration-earth-200 underline-offset-4 transition-all">Galéria megtekintése &rarr;</RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Scoped styles can be minimal since we use Tailwind and global utility classes */
</style>
