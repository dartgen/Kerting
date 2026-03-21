<script setup lang="ts">
import GardenerSlider from '@/components/GardenerSlider.vue';
import LatestWorks from '@/components/LatestWorks.vue';
import { motion } from 'motion-v';

const MotionDiv = motion.div;
</script>

<template>
  <div class="flex-1 flex flex-col items-center justify-between w-full h-full min-h-0 overflow-hidden py-1 md:py-2 gap-2">

    <!-- Hero Text -->
    <MotionDiv
      :initial="{ opacity: 0, y: -20 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.8, ease: 'easeOut' }"
      class="shrink-0 text-center w-full max-w-4xl px-4 py-3 md:px-6 md:py-4 bg-earth-950/60 backdrop-blur-md rounded-xl border border-earth-400/30 shadow-xl"
    >
      <h1 class="font-serif text-xl sm:text-2xl md:text-4xl text-earth-100 mb-1 md:mb-2 drop-shadow-[0_2px_4px_rgba(0,0,0,0.8)] tracking-wide">
        Üdvözöljük a Kerting oldalán!
      </h1>
      <p class="text-earth-50 text-[10px] sm:text-xs md:text-sm md:leading-snug font-light">
        Fedezze fel a legjobb kertészeti szakembereket és a legfrissebb kerti munkákat.<br class="hidden md:block"/>
        Böngésszen projektjeink között, olvassa el a véleményeket, és találja meg az<br class="hidden md:block"/>
        Önhöz legközelebbi szakértőt!
      </p>
    </MotionDiv>

    <!-- Slider Section -->
    <div class="w-full max-w-7xl flex-1 flex flex-col justify-center min-h-0">
       <GardenerSlider />
    </div>

    <!-- Latest Works -->
    <div class="w-full max-w-[1400px] shrink-0">
        <LatestWorks />
    </div>

  </div>
</template>

<style scoped>
</style>
