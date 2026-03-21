<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useGardenStore } from '@/stores/gardenStore';
import { motion } from 'motion-v';

const MotionDiv = motion.div;

const store = useGardenStore();
const cards = computed(() => store.professionals);

const currentIndex = ref(2);
let intervalId: number | null = null;

const getCardStyle = (index: number) => {
  const totalCards = cards.value.length;
  // Üres lista kezelése
  if (totalCards === 0) return {};

  let distance = index - currentIndex.value;

  if (distance > totalCards / 2) distance -= totalCards;
  if (distance < -totalCards / 2) distance += totalCards;

  // Vizuális állapotok
  if (distance === 0) {
    return { scale: 1.1, opacity: 1, zIndex: 10, filter: 'blur(0px)' };
  }
  if (Math.abs(distance) === 1) {
    return { scale: 0.9, opacity: 0.8, zIndex: 5, filter: 'blur(1px)' };
  }
  return { scale: 0.75, opacity: 0.5, zIndex: 1, filter: 'blur(3px)' };
};

const getCardOrder = (index: number) => {
  const totalCards = cards.value.length;
  if (totalCards === 0) return 0;
  let distance = index - currentIndex.value;
  if (distance > totalCards / 2) distance -= totalCards;
  if (distance < -totalCards / 2) distance += totalCards;

  return distance + totalCards;
};

const rotateCarousel = () => {
    if (cards.value.length)
        currentIndex.value = (currentIndex.value + 1) % cards.value.length;
};

onMounted(() => {
  intervalId = setInterval(rotateCarousel, 3000);
});

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId);
});
</script>

<template>
  <div class="w-full flex flex-col justify-center relative py-2 lg:py-0 h-full">
    <!-- Egységes kártyacsúszka asztali és mobil nézetre -->
    <div class="flex justify-center items-center gap-2 md:gap-6 h-full relative w-full perspective-1000 min-h-[280px]">
      <MotionDiv
        v-for="(pro, index) in cards"
        :key="pro.id"
        class="absolute md:relative flex flex-col items-center bg-white rounded-xl shadow-2xl p-3 md:p-6 w-[180px] md:w-[240px] lg:w-[280px] h-[240px] md:h-[320px] lg:h-[380px] shrink-0 border border-earth-400/30"
        :style="{ order: getCardOrder(index) }"
        layout
        :initial="false"
        :animate="getCardStyle(index)"
        :transition="{ type: 'spring', stiffness: 200, damping: 25 }"
      >
        <div
          class="w-full h-12 md:h-20 lg:h-24 flex items-center justify-center rounded-md mb-2 md:mb-4 text-2xl md:text-4xl overflow-hidden shadow-inner shrink-0"
          :class="getCardStyle(index).scale === 1.1 ? 'bg-earth-100 text-earth-600' : 'bg-earth-800 text-earth-200'"
        >
          <i class="pi pi-image text-xl md:text-3xl"></i>
        </div>

        <h3 class="text-sm md:text-xl font-bold mb-1 text-earth-900 truncate w-full text-center">{{ pro.name }}</h3>
        <div class="text-yellow-500 mb-1 md:mb-2 text-xs md:text-lg">★★★★★</div>
        <p class="text-[10px] md:text-sm text-gray-600 text-left line-clamp-3 md:line-clamp-4 leading-snug md:leading-relaxed mb-auto w-full px-1 md:px-2">{{ pro.desc }}</p>

        <button class="mt-2 md:mt-4 w-full py-1.5 md:py-2 uppercase text-[10px] md:text-sm font-bold tracking-wide bg-gradient-to-r from-earth-500 to-earth-700 text-white rounded hover:from-earth-500 hover:to-earth-600 transition-all shadow-md">
          Profil megtekintése
        </button>
      </MotionDiv>
    </div>

    <div class="hidden md:block text-center mt-2 z-20 shrink-0">
      <button class="bg-earth-800 text-earth-100 px-6 py-2 rounded-full border border-earth-600 font-bold hover:bg-earth-700 transition-colors shadow-lg text-sm">
        További szakemberek &gt;
      </button>
    </div>
  </div>
</template>

<style scoped>
</style>
